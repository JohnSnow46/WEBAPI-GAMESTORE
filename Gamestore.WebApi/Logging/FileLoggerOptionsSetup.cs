using Microsoft.Extensions.Options;

namespace Gamestore.WebApi.Logging;

/// <summary>
/// Sets up default options for FileLoggerOptions.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FileLoggerOptionsSetup"/> class.
/// </remarks>
/// <param name="configuration">The configuration to bind to.</param>
public class FileLoggerOptionsSetup(IConfiguration configuration) : ConfigureFromConfigurationOptions<FileLoggerOptions>(configuration.GetSection("Logging:File"))
{
}