using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Gamestore.WebApi.Logging;

[ProviderAlias("File")]
public class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggerOptions _options;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
    private bool _disposed;

    public FileLoggerProvider(IOptions<FileLoggerOptions> options)
    {
        _options = options.Value;
        EnsureLogDirectoryExists();
    }

    public ILogger CreateLogger(string categoryName)
    {
        ThrowIfDisposed();
        return _loggers.GetOrAdd(categoryName, name =>
            new FileLogger(name, _options, () => GetCurrentLogFileName(name)));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _loggers.Clear();
        }

        _disposed = true;
    }

    private string GetCurrentLogFileName(string categoryName)
    {
        var date = GetCurrentDate();

        var sanitizedCategoryName = SanitizeCategoryName(categoryName);
        var fileName = $"{_options.FileNamePrefix}{sanitizedCategoryName}_{date}{_options.FileExtension}";
        return Path.Combine(_options.LogDirectory, fileName);
    }

    private string GetCurrentDate()
    {
        var now = _options.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;
        return now.ToString("yyyy-MM-dd");
    }

    private static string SanitizeCategoryName(string categoryName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", categoryName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        var parts = sanitized.Split('.');
        return parts.Length > 0 ? parts[^1] : "Log";
    }

    private void EnsureLogDirectoryExists()
    {
        if (!Directory.Exists(_options.LogDirectory))
        {
            Directory.CreateDirectory(_options.LogDirectory);
        }
    }

    private void ThrowIfDisposed()
    {
        if (!_disposed)
        {
            return;
        }

        throw new ObjectDisposedException(nameof(FileLoggerProvider));
    }
}