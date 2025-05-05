using System.Text;

namespace Gamestore.WebApi.Logging;

/// <summary>
/// Service for detailed exception logging.
/// </summary>
public class ErrorLoggingService(ILogger<ErrorLoggingService> logger)
{
    private readonly ILogger<ErrorLoggingService> _logger = logger;

    /// <summary>
    /// Logs detailed information about an exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="additionalInfo">Additional contextual information (optional).</param>
    public void LogException(Exception exception, string additionalInfo = null)
    {
        if (exception == null)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("=== EXCEPTION DETAILS ===");
        sb.AppendLine($"Exception Type: {exception.GetType().FullName}");
        sb.AppendLine($"Message: {exception.Message}");
        sb.AppendLine($"Source: {exception.Source}");

        if (!string.IsNullOrEmpty(additionalInfo))
        {
            sb.AppendLine($"Additional Info: {additionalInfo}");
        }

        AppendExceptionSpecificDetails(sb, exception);

        if (exception.InnerException != null)
        {
            AppendInnerExceptions(sb, exception.InnerException);
        }

        sb.AppendLine("Stack Trace:");
        sb.AppendLine(exception.StackTrace);
        sb.AppendLine("=== END OF EXCEPTION DETAILS ===");

        _logger.LogError(exception, sb.ToString());
    }

    /// <summary>
    /// Adds details specific to the type of exception.
    /// </summary>
    private static void AppendExceptionSpecificDetails(StringBuilder sb, Exception exception)
    {
        switch (exception)
        {
            case AggregateException aggregateEx:
                sb.AppendLine($"Number of aggregated exceptions: {aggregateEx.InnerExceptions.Count}");
                break;

            case ArgumentException argEx:
                sb.AppendLine($"Parameter name: {argEx.ParamName}");
                break;

            case InvalidOperationException:
                sb.AppendLine("Operation is invalid in the current state of the object");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Adds information about inner exceptions.
    /// </summary>
    private static void AppendInnerExceptions(StringBuilder sb, Exception innerException, int level = 1)
    {
        sb.AppendLine($"=== INNER EXCEPTION (level {level}) ===");
        sb.AppendLine($"Type: {innerException.GetType().FullName}");
        sb.AppendLine($"Message: {innerException.Message}");
        sb.AppendLine($"Source: {innerException.Source}");

        if (innerException.InnerException != null)
        {
            AppendInnerExceptions(sb, innerException.InnerException, level + 1);
        }
    }
}
