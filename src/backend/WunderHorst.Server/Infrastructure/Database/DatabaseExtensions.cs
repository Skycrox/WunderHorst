namespace WunderHorst.Server.Infrastructure.Database;

/// <summary>
/// Dependency injection and startup extensions for SQLite and Dapper.AOT.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Registers <see cref="DatabaseOptions"/>, <see cref="ISqliteConnectionFactory"/>,
    /// <see cref="SqlMigrationRunner"/>, and the SQLite health check.
    /// </summary>
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
        builder.Services.AddSingleton<ISqliteConnectionFactory, SqliteConnectionFactory>();
        builder.Services.AddSingleton<SqlMigrationRunner>();
        builder.Services.AddHealthChecks().AddCheck<SqliteHealthCheck>("sqlite");

        return builder;
    }

    /// <summary>
    /// Executes pending SQLite database migrations at application startup.
    /// </summary>
    public static async Task InitializeDatabaseAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        var runner = app.Services.GetRequiredService<SqlMigrationRunner>();
        await runner.MigrateAsync(cancellationToken);
    }
}
