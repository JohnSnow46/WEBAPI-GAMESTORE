namespace Gamestore.WebApi.Logging;

public class FileLoggerOptions
{
    public string LogDirectory { get; set; } = "logs";

    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    public long FileSizeLimit { get; set; } = 10 * 10 * 1024;

    public int RetainedFileCountLimit { get; set; } = 31;

    public bool UseUtcTimestamp { get; set; }

    public string FileNamePrefix { get; set; } = "app_";

    public string FileExtension { get; set; } = ".txt";
}
