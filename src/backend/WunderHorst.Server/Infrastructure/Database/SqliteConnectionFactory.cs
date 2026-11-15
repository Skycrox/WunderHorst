using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace WunderHorst.Server.Infrastructure.Database;

/// <summary>
/// Abstraction for creating and opening configured SQLite connections.
/// </summary>
public interface ISqliteConnectionFactory
{
    /// <summary>
    /// Opens a new SQLite connection with foreign keys and WAL mode configured.
    /// </summary>
    ValueTask<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Default <see cref="ISqliteConnectionFactory"/> implementation using <see cref="DatabaseOptions"/>.
/// </summary>
public sealed class SqliteConnectionFactory(IOptions<DatabaseOptions> options) : ISqliteConnectionFactory
{
    private readonly string _connectionString = options.Value.ConnectionString;

    /// <inheritdoc />
    public async ValueTask<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        EnsureDirectoryExists(_connectionString);

        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");

        if (!IsInMemoryConnection(_connectionString))
        {
            await connection.ExecuteAsync("PRAGMA journal_mode = WAL;");
        }

        return connection;
    }

    internal static void EnsureDirectoryExists(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = builder.DataSource;

        if (string.IsNullOrWhiteSpace(dataSource) ||
            string.Equals(dataSource, ":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    internal static bool IsInMemoryConnection(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        return string.Equals(builder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase)
            || builder.Mode == SqliteOpenMode.Memory;
    }
}
