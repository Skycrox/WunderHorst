using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WunderHorst.Server.Features.System.GetHealth;
using WunderHorst.Server.Infrastructure.Database;
using WunderHorst.Server.Infrastructure.Hosting;
using WunderHorst.Server.Infrastructure.Serialization;

namespace WunderHorst.Server.Tests.Infrastructure.Database;

public sealed class DatabaseTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(
        Path.GetTempPath(),
        "wunderhorst-tests-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task OpenConnectionAsync_WithFileDatabase_CreatesDirectoryAndConfiguresPragmas()
    {
        var dbPath = Path.Combine(_tempDir, "nested", "test.db");
        var factory = new SqliteConnectionFactory(Options.Create(new DatabaseOptions
        {
            ConnectionString = $"Data Source={dbPath}"
        }));

        await using var connection = await factory.OpenConnectionAsync();

        Assert.True(Directory.Exists(Path.GetDirectoryName(dbPath)));
        var fk = await connection.QuerySingleAsync<long>("PRAGMA foreign_keys;");
        var mode = await connection.QuerySingleAsync<string>("PRAGMA journal_mode;");
        Assert.Equal(1, fk);
        Assert.Equal("wal", mode, ignoreCase: true);
    }

    [Fact]
    public async Task OpenConnectionAsync_WithInMemoryDatabase_ConfiguresForeignKeys()
    {
        var factory = new SqliteConnectionFactory(Options.Create(new DatabaseOptions
        {
            ConnectionString = "Data Source=:memory:"
        }));

        await using var connection = await factory.OpenConnectionAsync();

        var fk = await connection.QuerySingleAsync<long>("PRAGMA foreign_keys;");
        Assert.Equal(1, fk);
    }

    [Fact]
    public async Task MigrateAsync_WhenCalledMultipleTimes_IsIdempotentAndCreatesTables()
    {
        var dbPath = Path.Combine(_tempDir, "migrations.db");
        var factory = new SqliteConnectionFactory(Options.Create(new DatabaseOptions
        {
            ConnectionString = $"Data Source={dbPath}"
        }));
        var runner = new SqlMigrationRunner(factory, NullLogger<SqlMigrationRunner>.Instance);

        await runner.MigrateAsync();
        await using var connection = await factory.OpenConnectionAsync();
        var migrationsFirst = (await connection.QueryAsync<string>("SELECT id FROM schema_migrations;")).ToList();

        await runner.MigrateAsync();
        var migrationsSecond = (await connection.QueryAsync<string>("SELECT id FROM schema_migrations;")).ToList();
        var tableExists = await connection.QuerySingleAsync<long>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='app_metadata';");

        Assert.Contains("001_init.sql", migrationsFirst);
        Assert.Equal(migrationsFirst.Count, migrationsSecond.Count);
        Assert.Equal(1, tableExists);
    }

    [Fact]
    public async Task SqliteHealthCheck_WhenDatabaseIsReachable_ReturnsHealthy()
    {
        var dbPath = Path.Combine(_tempDir, "health.db");
        var factory = new SqliteConnectionFactory(Options.Create(new DatabaseOptions
        {
            ConnectionString = $"Data Source={dbPath}"
        }));
        var check = new SqliteHealthCheck(factory);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task SqliteHealthCheck_WhenConnectionThrows_ReturnsUnhealthy()
    {
        var check = new SqliteHealthCheck(new FailingConnectionFactory());

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task GetHealthEndpoint_WithDatabaseRegistered_IncludesSqliteCheck()
    {
        var dbPath = Path.Combine(_tempDir, "integration-health.db");
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = $"Data Source={dbPath}"
        });
        builder.AddDatabase();
        builder.Services.AddAuthorization();
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        await using var app = builder.Build();
        await app.InitializeDatabaseAsync();
        app.UseAuthorization();
        app.Use(async (context, next) =>
        {
            context.Connection.LocalPort = 18420;
            await next();
        });
        app.MapWebApi(new ServerOptions()).MapGetHealth();

        await app.StartAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/api/health");
        response.EnsureSuccessStatusCode();

        var payload = await System.Net.Http.Json.HttpContentJsonExtensions.ReadFromJsonAsync(
            response.Content,
            AppJsonSerializerContext.Default.GetHealthResponse);

        Assert.NotNull(payload);
        Assert.Equal("Healthy", payload!.Status);
        Assert.Equal("Healthy", payload.Checks["sqlite"]);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    private sealed class FailingConnectionFactory : ISqliteConnectionFactory
    {
        public ValueTask<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated connection failure");
        }
    }
}
