using Serilog;
using Serilog.Events;

namespace WunderHorst.Server.Infrastructure.Observability;

/// <summary>
/// Extension methods for configuring AOT-compatible Serilog and OpenTelemetry observability.
/// </summary>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Applies AOT-bound <see cref="LoggingOptions"/> to a Serilog <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static LoggerConfiguration ConfigureLogger(
        this LoggerConfiguration loggerConfiguration,
        LoggingOptions options)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);
        ArgumentNullException.ThrowIfNull(options);

        var minLevel = LoggingOptions.ParseLevel(options.MinimumLevel, LogEventLevel.Information);
        var aspNetLevel = LoggingOptions.ParseLevel(options.MicrosoftAspNetCoreLevel, LogEventLevel.Warning);

        loggerConfiguration
            .MinimumLevel.Is(minLevel)
            .MinimumLevel.Override("Microsoft.AspNetCore", aspNetLevel)
            .Enrich.FromLogContext();

        if (options.WriteToConsole)
        {
            loggerConfiguration.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        }

        return loggerConfiguration;
    }

    /// <summary>
    /// Registers AOT-compatible Serilog logging and OpenTelemetry tracing/metrics.
    /// </summary>
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var loggingOptions = new LoggingOptions();
        builder.Configuration.GetSection(LoggingOptions.SectionName).Bind(loggingOptions);
        builder.Services.Configure<LoggingOptions>(builder.Configuration.GetSection(LoggingOptions.SectionName));

        builder.Services.AddSerilog((_, loggerConfig) => loggerConfig.ConfigureLogger(loggingOptions));

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource(TelemetryInstrumentation.ActivitySourceName))
            .WithMetrics(metrics => metrics.AddMeter(TelemetryInstrumentation.MeterName));

        return builder;
    }
}
