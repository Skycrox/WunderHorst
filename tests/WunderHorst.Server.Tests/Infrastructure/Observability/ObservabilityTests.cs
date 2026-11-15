using Serilog;
using Serilog.Events;
using WunderHorst.Server.Infrastructure.Observability;

namespace WunderHorst.Server.Tests.Infrastructure.Observability;

public sealed class ObservabilityTests
{
    [Theory]
    [InlineData("Debug", LogEventLevel.Debug)]
    [InlineData("warning", LogEventLevel.Warning)]
    [InlineData("Error", LogEventLevel.Error)]
    public void ParseLevel_WhenValidString_ReturnsParsedLevel(string input, LogEventLevel expected)
    {
        var result = LoggingOptions.ParseLevel(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("NotALevel")]
    public void ParseLevel_WhenInvalidOrNull_ReturnsFallback(string? input)
    {
        var result = LoggingOptions.ParseLevel(input, LogEventLevel.Warning);
        Assert.Equal(LogEventLevel.Warning, result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConfigureLogger_WhenConfigured_CreatesWorkingLogger(bool writeToConsole)
    {
        var options = new LoggingOptions
        {
            MinimumLevel = "Information",
            MicrosoftAspNetCoreLevel = "Warning",
            WriteToConsole = writeToConsole
        };

        using var logger = new LoggerConfiguration()
            .ConfigureLogger(options)
            .CreateLogger();

        Assert.True(logger.IsEnabled(LogEventLevel.Information));
        Assert.False(logger.IsEnabled(LogEventLevel.Debug));
    }

    [Fact]
    public void TelemetryInstrumentation_HasExpectedSourceAndMeterNames()
    {
        Assert.Equal("WunderHorst.Server", TelemetryInstrumentation.ActivitySource.Name);
        Assert.Equal("WunderHorst.Server", TelemetryInstrumentation.Meter.Name);
    }
}
