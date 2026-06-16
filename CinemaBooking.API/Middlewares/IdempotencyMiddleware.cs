using System.Collections.Concurrent;

namespace CinemaBooking.API.Middlewares;

public class IdempotencyMiddleware
{
    private static readonly ConcurrentDictionary<string, CachedResponse> _responses = new();
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext.Request.Method != HttpMethods.Post || ShouldSkip(httpContext.Request.Path))
        {
            await _next(httpContext);
            return;
        }

        if (!httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var key) ||
           string.IsNullOrWhiteSpace(key.ToString()))
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new { ErrorMessage = "Idempotency-Key header is missing!" });
            return;
        }

        var cacheKey = $"{httpContext.Request.Method}:{httpContext.Request.Path}:{key}";

        if (_responses.TryGetValue(cacheKey, out var cachedResponse))
        {
            httpContext.Response.Headers["Idempotency-Cache"] = "HIT";
            httpContext.Response.StatusCode = cachedResponse.StatusCode;
            httpContext.Response.ContentType = cachedResponse.ContentType;
            await httpContext.Response.Body.WriteAsync(cachedResponse.Body);
            return;
        }

        await using var memoryStream = new MemoryStream();
        var originalBody = httpContext.Response.Body;
        httpContext.Response.Body = memoryStream;

        try
        {

            await _next(httpContext);

            memoryStream.Seek(0, SeekOrigin.Begin);

            if (httpContext.Response.StatusCode >= 200 && httpContext.Response.StatusCode < 300)
            {
                var cached = new CachedResponse(
                    memoryStream.ToArray(),
                    httpContext.Response.ContentType ?? "application/json",
                    httpContext.Response.StatusCode);

                _responses.TryAdd(cacheKey, cached);
            }

            await memoryStream.CopyToAsync(originalBody);
        }
        finally
        {
            httpContext.Response.Body = originalBody;
        }
    }

    private static bool ShouldSkip(PathString path)
        => path.StartsWithSegments("/auth");
}

public record CachedResponse(byte[] Body, string ContentType, int StatusCode);

public static class IdempotencyMiddlewareExtensions
{
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
        => builder.UseMiddleware<IdempotencyMiddleware>();
}