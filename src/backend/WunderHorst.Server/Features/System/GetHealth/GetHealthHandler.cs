using Microsoft.Extensions.Diagnostics.HealthChecks;
using WunderHorst.Server.Infrastructure.Serialization;

namespace WunderHorst.Server.Features.System.GetHealth;

/// <summary>
/// Mapping extensions for <see cref="HealthReport"/>.
/// </summary>
public static class HealthReportMapperExtensions
{
    /// <summary>
    /// Maps a <see cref="HealthReport"/> to a <see cref="GetHealthResponse"/>.
    /// </summary>
    public static GetHealthResponse ToResponse(this HealthReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var checks = new Dictionary<string, string>(report.Entries.Count, StringComparer.Ordinal);
        foreach (var entry in report.Entries)
        {
            checks[entry.Key] = entry.Value.Status.ToString();
        }

        return new GetHealthResponse(report.Status.ToString(), checks);
    }
}

/// <summary>
/// Handler for retrieving current application and dependency health status.
/// </summary>
public static class GetHealthHandler
{
    /// <summary>
    /// Executes registered health checks and returns <c>200 OK</c> or <c>503 Service Unavailable</c>.
    /// </summary>
    public static async Task<IResult> HandleAsync(
        HealthCheckService healthCheckService,
        CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);
        var response = report.ToResponse();

        return report.Status == HealthStatus.Unhealthy
            ? Results.Json(
                response,
                AppJsonSerializerContext.Default.GetHealthResponse,
                statusCode: StatusCodes.Status503ServiceUnavailable)
            : Results.Ok(response);
    }
}
