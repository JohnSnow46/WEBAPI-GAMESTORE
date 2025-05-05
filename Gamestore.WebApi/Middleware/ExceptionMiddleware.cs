using System.Net;
using System.Text.Json;
using Gamestore.Entities.ErrorModels;
using Gamestore.WebApi.Logging;

namespace Gamestore.WebApi.Middleware;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger,
    IHostEnvironment environment)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;
    private readonly IHostEnvironment _environment = environment;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public async Task InvokeAsync(HttpContext httpContext, ErrorLoggingService errorLogger)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex, errorLogger);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, ErrorLoggingService errorLogger)
    {
        _logger.LogError(
            exception,
            "Unhandled exception: {ExceptionType} - {ExceptionMessage}",
            exception.GetType().Name,
            exception.Message);

        var requestPath = context.Request.Path;
        errorLogger.LogException(exception, $"Occurred while processing: {context.Request.Method} {requestPath}");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ErrorResponseModel
        {
            ErrorId = Guid.NewGuid().ToString(),
            StatusCode = context.Response.StatusCode,
            Message = GetUserFriendlyMessage(exception),
        };

        if (_environment.IsDevelopment())
        {
            response.Details = exception.ToString();
        }

        var json = JsonSerializer.Serialize(response, _jsonOptions);

        await context.Response.WriteAsync(json);
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentException => "Invalid input data. Please check your information and try again.",
            InvalidOperationException => "This operation cannot be performed in the current state.",
            UnauthorizedAccessException => "You don't have the required permissions to perform this operation.",
            TimeoutException => "The operation timed out. Please try again later.",
            _ => $"An unexpected error occurred. If the problem persists, please contact the administrator and provide the error ID.",
        };
    }
}