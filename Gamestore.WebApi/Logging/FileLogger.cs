using System.Text;

namespace Gamestore.WebApi.Logging;

public class FileLogger(string name, FileLoggerOptions options, Func<string> getCurrentFileName) : ILogger
{
    private readonly string _name = name;
    private readonly FileLoggerOptions _options = options;
    private readonly Func<string> _getCurrentFileName = getCurrentFileName;
    private readonly object _lock = new();

    public IDisposable BeginScope<TState>(TState state) => default;

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _options.MinimumLogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);

        if (string.IsNullOrEmpty(message) && exception == null)
        {
            return;
        }

        var logBuilder = new StringBuilder();
        var timestamp = _options.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;

        // Format: [Timestamp] [LogLevel] [Category] Message
        logBuilder.Append($"[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] ");
        logBuilder.Append($"[{GetLogLevelString(logLevel)}] ");
        logBuilder.Append($"[{_name}] ");
        logBuilder.Append(message);

        if (exception != null)
        {
            logBuilder.AppendLine();
            logBuilder.Append($"Exception: {exception.GetType().FullName}: {exception.Message}");
            logBuilder.AppendLine();

            if (exception.InnerException != null)
            {
                logBuilder.Append($"Inner Exception: {exception.InnerException.GetType().FullName}: {exception.InnerException.Message}");
                logBuilder.AppendLine();
            }

            logBuilder.Append($"Stack Trace: {exception.StackTrace}");
        }

        logBuilder.AppendLine();

        var filePath = _getCurrentFileName();
        var directoryName = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        lock (_lock)
        {
            File.AppendAllText(filePath, logBuilder.ToString());
        }
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRIT",
            LogLevel.None => throw new NotImplementedException(),
            _ => "NONE",
        };
    }
}
