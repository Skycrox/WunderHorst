using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace WunderHorst.Server.Infrastructure.Observability;

/// <summary>
/// Central OpenTelemetry instrumentation primitives for WunderHorst.
/// </summary>
public static class TelemetryInstrumentation
{
    public const string ServiceName = "WunderHorst.Server";
    public const string ActivitySourceName = "WunderHorst.Server";
    public const string MeterName = "WunderHorst.Server";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    public static readonly Meter Meter = new(MeterName);
}
