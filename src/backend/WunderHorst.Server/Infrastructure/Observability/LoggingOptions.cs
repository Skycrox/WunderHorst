using Serilog.Events;

namespace WunderHorst.Server.Infrastructure.Observability;

/// <summary>
/// AOT-bound logging configuration options for Serilog.
/// </summary>
public sealed class LoggingOptions
{
    public const string SectionName = "Observability:Logging";

    /// <summary>
    /// Minimum log level name (e.g. Verbose, Debug, Information, Warning, Error, Fatal).
    /// </summary>
    public string MinimumLevel { get; set; } = nameof(LogEventLevel.Information);

    /// <summary>
    /// Minimum log level for Microsoft.AspNetCore namespace.
    /// </summary>
    public string MicrosoftAspNetCoreLevel { get; set; } = nameof(LogEventLevel.Warning);

    /// <summary>
    /// Enables console sink output.
    /// </summary>
    public bool WriteToConsole { get; set; } = true;

    /// <summary>
    /// Parses a configured log level string safely with fallback to <see cref="LogEventLevel.Information"/>.
    /// </summary>
    public static LogEventLevel ParseLevel(string? value, LogEventLevel fallback = LogEventLevel.Information)
    {
        return Enum.TryParse<LogEventLevel>(value, ignoreCase: true, out var parsed)
            ? parsed
            : fallback;
    }
}
