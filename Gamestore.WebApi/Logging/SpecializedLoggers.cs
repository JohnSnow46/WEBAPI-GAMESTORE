namespace Gamestore.WebApi.Logging;

/// <summary>
/// Helper class to maintain separate loggers for different types of logs.
/// </summary>
public static class SpecializedLoggers
{
    /// <summary>
    /// Creates a logger specifically for request logging.
    /// </summary>
    /// <param name="factory">The logger factory.</param>
    /// <returns>A logger for request information.</returns>
    public static ILogger CreateRequestLogger(ILoggerFactory factory)
    {
        return factory.CreateLogger("RequestLogs");
    }

    /// <summary>
    /// Creates a logger specifically for error logging.
    /// </summary>
    /// <param name="factory">The logger factory.</param>
    /// <returns>A logger for error information.</returns>
    public static ILogger CreateErrorLogger(ILoggerFactory factory)
    {
        return factory.CreateLogger("ErrorLogs");
    }

    /// <summary>
    /// Creates a logger specifically for business layer logging.
    /// </summary>
    /// <param name="factory">The logger factory.</param>
    /// <param name="category">The business layer category.</param>
    /// <returns>A logger for business layer information.</returns>
    public static ILogger CreateBusinessLogger(ILoggerFactory factory, string category)
    {
        return factory.CreateLogger($"Business.{category}");
    }

    /// <summary>
    /// Extension method to log request information.
    /// </summary>
    public static void LogRequest(this ILogger logger, string ipAddress, string url, int statusCode, string requestContent, string responseContent, long elapsedMs)
    {
        logger.LogInformation(
            "Request: {IpAddress} | {Url} | Status: {StatusCode} | Duration: {ElapsedMs}ms | Request: {RequestContent} | Response: {ResponseContent}",
            ipAddress,
            url,
            statusCode,
            elapsedMs,
            requestContent?.Length > 1000 ? string.Concat(requestContent.AsSpan(0, 1000), "...") : requestContent,
            responseContent?.Length > 1000 ? string.Concat(responseContent.AsSpan(0, 1000), "...") : responseContent);
    }

    /// <summary>
    /// Extension method to log exception details.
    /// </summary>
    public static void LogExceptionDetail(this ILogger logger, Exception exception, string additionalInfo = null)
    {
        logger.LogError(
            exception,
            "Exception: {ExceptionType} | Message: {ExceptionMessage} | Additional Info: {AdditionalInfo}",
            exception.GetType().Name,
            exception.Message,
            additionalInfo ?? "None");
    }
}