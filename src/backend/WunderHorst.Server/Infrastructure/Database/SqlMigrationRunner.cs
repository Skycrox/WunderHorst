using System.Reflection;
using Dapper;
using Microsoft.Data.Sqlite;

namespace WunderHorst.Server.Infrastructure.Database;

/// <summary>
/// Deterministic, idempotent runner for embedded SQLite migration scripts.
/// </summary>
public sealed class SqlMigrationRunner(
    ISqliteConnectionFactory connectionFactory,
    ILogger<SqlMigrationRunner> logger)
{
    private const string MigrationsPrefix = "WunderHorst.Server.Infrastructure.Database.Migrations.";

    /// <summary>
    /// Applies all pending embedded <c>.sql</c> migrations in lexical order.
    /// </summary>
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            """
            CREATE TABLE IF NOT EXISTS schema_migrations (
                id TEXT PRIMARY KEY,
                applied_at TEXT NOT NULL
            );
            """);

        var applied = (await connection.QueryAsync<string>(
            "SELECT id FROM schema_migrations;")).ToHashSet(StringComparer.Ordinal);

        var assembly = typeof(SqlMigrationRunner).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(MigrationsPrefix, StringComparison.Ordinal) &&
                           name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        foreach (var resourceName in resourceNames)
        {
            var migrationId = resourceName[MigrationsPrefix.Length..];
            if (applied.Contains(migrationId))
            {
                continue;
            }

            var sql = await ReadEmbeddedSqlAsync(assembly, resourceName, cancellationToken);

            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
            await connection.ExecuteAsync(sql, transaction: transaction);
            await connection.ExecuteAsync(
                "INSERT INTO schema_migrations (id, applied_at) VALUES (@Id, @AppliedAt);",
                new { Id = migrationId, AppliedAt = DateTimeOffset.UtcNow.ToString("O") },
                transaction: transaction);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Applied SQLite migration {MigrationId}", migrationId);
        }
    }

    private static async Task<string> ReadEmbeddedSqlAsync(
        Assembly assembly,
        string resourceName,
        CancellationToken cancellationToken)
    {
        await using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded migration resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
