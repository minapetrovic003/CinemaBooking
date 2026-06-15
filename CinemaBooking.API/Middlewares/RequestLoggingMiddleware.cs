namespace CinemaBooking.API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var start = DateTime.UtcNow;

        _logger.LogInformation("→ [{Method}] {Path} | Started: {Time}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            start.ToString("HH:mm:ss.fff"));

        await _next(httpContext);

        var duration = (DateTime.UtcNow - start).TotalMilliseconds;

        _logger.LogInformation("← [{Method}] {Path} | Status: {StatusCode} | Duration: {Duration}ms",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.Response.StatusCode,
            duration.ToString("F1"));
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        => builder.UseMiddleware<RequestLoggingMiddleware>();
}