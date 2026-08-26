using System.Net;
using System.Text.Json;

namespace MovieRecommendation.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(
                context,
                ex);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            KeyNotFoundException =>
                HttpStatusCode.NotFound,

            ArgumentException =>
                HttpStatusCode.BadRequest,

            InvalidOperationException =>
                HttpStatusCode.Conflict,

            UnauthorizedAccessException =>
                HttpStatusCode.Unauthorized,

            _ =>
                HttpStatusCode.InternalServerError
        };

        if (statusCode ==
            HttpStatusCode.InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred.");
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Handled exception occurred. StatusCode: {StatusCode}",
                (int)statusCode);
        }

        var response = new
        {
            statusCode = (int)statusCode,

            message =
                statusCode == HttpStatusCode.InternalServerError
                    ? "An unexpected error occurred."
                    : exception.Message
        };

        context.Response.StatusCode =
            (int)statusCode;

        context.Response.ContentType =
            "application/json";

        var json =
            JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}