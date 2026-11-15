using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WunderHorst.Server.Infrastructure.Database;

/// <summary>
/// Health check verifying SQLite database connectivity via <c>SELECT 1</c>.
/// </summary>
public sealed class SqliteHealthCheck(ISqliteConnectionFactory connectionFactory) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
            var result = await connection.ExecuteScalarAsync<int>("SELECT 1;");

            return result == 1
                ? HealthCheckResult.Healthy("SQLite connection succeeded.")
                : HealthCheckResult.Unhealthy("SQLite returned unexpected probe value.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQLite connection failed.", ex);
        }
    }
}
