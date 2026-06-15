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
        if (httpContext.Request.Method != HttpMethods.Post)
        {
            await _next(httpContext);
            return;
        }

        if (!httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var key))
        {
            httpContext.Response.StatusCode = 400;
            await httpContext.Response.WriteAsJsonAsync(new { ErrorMessage = "Idempotency-Key header is missing!" });
            return;
        }

        if (_responses.TryGetValue(key!, out var cachedResponse))
        {
            httpContext.Response.Headers["Idempotency-Cache"] = "HIT";
            httpContext.Response.StatusCode = cachedResponse.StatusCode;
            httpContext.Response.ContentType = cachedResponse.ContentType;
            await httpContext.Response.Body.WriteAsync(cachedResponse.Body);
            return;
        }

        var memoryStream = new MemoryStream();
        var originalBody = httpContext.Response.Body;
        httpContext.Response.Body = memoryStream;

        await _next(httpContext);

        memoryStream.Seek(0, SeekOrigin.Begin);

        if (httpContext.Response.StatusCode >= 200 && httpContext.Response.StatusCode < 300)
        {
            var cached = new CachedResponse(
                memoryStream.ToArray(),
                httpContext.Response.ContentType ?? "application/json",
                httpContext.Response.StatusCode);

            _responses.TryAdd(key!, cached);
        }

        await memoryStream.CopyToAsync(originalBody);
        httpContext.Response.Body = originalBody;
    }
}

public record CachedResponse(byte[] Body, string ContentType, int StatusCode);

public static class IdempotencyMiddlewareExtensions
{
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
        => builder.UseMiddleware<IdempotencyMiddleware>();
}