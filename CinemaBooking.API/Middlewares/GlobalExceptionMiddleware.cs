using FluentValidation;
using System.Net;
using System.Text.Json;

namespace CinemaBooking.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        object response;
        int statusCode;

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    StatusCode = statusCode,
                    Message = "Validation failed.",
                    Errors = validationEx.Errors.Select(e => new
                    {
                        Field = e.PropertyName,
                        Error = e.ErrorMessage
                    }),
                    Timestamp = DateTime.UtcNow
                };
                break;

            case ArgumentException argEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    StatusCode = statusCode,
                    Message = argEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                break;

            case KeyNotFoundException notFoundEx:
                statusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    StatusCode = statusCode,
                    Message = notFoundEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                break;

            case InvalidOperationException invEx:
                statusCode = (int)HttpStatusCode.Conflict;
                response = new
                {
                    StatusCode = statusCode,
                    Message = invEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                response = new
                {
                    StatusCode = statusCode,
                    Message = "An unexpected error occurred. Please try again later.",
                    Timestamp = DateTime.UtcNow
                };
                break;
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        => builder.UseMiddleware<GlobalExceptionMiddleware>();
}