using System.Net;
using System.Net.Http.Json;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WunderHorst.Server.Features.Auth.SetupAdmin;
using WunderHorst.Server.Infrastructure.Auth;
using WunderHorst.Server.Infrastructure.Database;
using WunderHorst.Server.Infrastructure.Hosting;
using WunderHorst.Server.Infrastructure.Serialization;

namespace WunderHorst.Server.Tests.Features.Auth;

public sealed class SetupAdminTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(
        Path.GetTempPath(),
        "wunderhorst-setup-tests-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsNoErrors()
    {
        var errors = SetupAdminValidator.Validate(new SetupAdminRequest("admin", "StrongPass123!"));
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("", "StrongPass123!", "username")]
    [InlineData(null, "StrongPass123!", "username")]
    [InlineData("ab", "StrongPass123!", "username")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "StrongPass123!", "username")]
    [InlineData("admin", "short", "password")]
    [InlineData("admin", "", "password")]
    [InlineData("admin", null, "password")]
    public void Validate_WhenRequestIsInvalid_ReturnsExpectedErrors(
        string? username,
        string? password,
        string expectedKey)
    {
        var errors = SetupAdminValidator.Validate(new SetupAdminRequest(username!, password!));
        Assert.True(errors.ContainsKey(expectedKey));
    }

    [Fact]
    public void Validate_WhenPasswordExceedsMaxLength_ReturnsPasswordError()
    {
        var tooLongPassword = new string('x', SetupAdminValidator.MaxPasswordLength + 1);
        var errors = SetupAdminValidator.Validate(new SetupAdminRequest("admin", tooLongPassword));
        Assert.True(errors.ContainsKey("password"));
    }

    [Fact]
    public void ToResponse_MapsAllFields()
    {
        var row = new UserRow("u1", "admin", "Admin", "2026-11-15T00:00:00Z");

        var response = row.ToResponse();

        Assert.Equal("u1", response.Id);
        Assert.Equal("admin", response.Username);
        Assert.Equal("Admin", response.Role);
        Assert.Equal("2026-11-15T00:00:00Z", response.CreatedAt);
    }

    [Fact]
    public async Task SetupEndpoints_CreateInitialAdminOnce_AndRejectSecondSetupWithConflict()
    {
        await using var app = await CreateTestAppAsync("setup-flow.db");
        using var client = app.GetTestClient();

        // 1. Initially needs setup
        var statusBefore = await client.GetFromJsonAsync(
            "/api/auth/setup-status",
            AppJsonSerializerContext.Default.SetupStatusResponse);
        Assert.NotNull(statusBefore);
        Assert.True(statusBefore!.NeedsSetup);

        // 2. Invalid payload -> 400 ValidationProblem
        var invalidResponse = await client.PostAsJsonAsync(
            "/api/auth/setup",
            new SetupAdminRequest("a", "123"),
            AppJsonSerializerContext.Default.SetupAdminRequest);
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);

        // 3. Valid setup -> 201 Created
        var createResponse = await client.PostAsJsonAsync(
            "/api/auth/setup",
            new SetupAdminRequest("  admin  ", "SuperSecret123!"),
            AppJsonSerializerContext.Default.SetupAdminRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync(
            AppJsonSerializerContext.Default.SetupAdminResponse);
        Assert.NotNull(created);
        Assert.Equal("admin", created!.Username);
        Assert.Equal("Admin", created.Role);

        // 4. Status now reports NeedsSetup == false
        var statusAfter = await client.GetFromJsonAsync(
            "/api/auth/setup-status",
            AppJsonSerializerContext.Default.SetupStatusResponse);
        Assert.False(statusAfter!.NeedsSetup);

        // 5. Second setup -> 409 Conflict
        var conflictResponse = await client.PostAsJsonAsync(
            "/api/auth/setup",
            new SetupAdminRequest("second", "SuperSecret123!"),
            AppJsonSerializerContext.Default.SetupAdminRequest);
        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);

        // 6. Password is stored hashed, never plaintext
        var factory = app.Services.GetRequiredService<ISqliteConnectionFactory>();
        await using var connection = await factory.OpenConnectionAsync();
        var storedHash = await connection.QuerySingleAsync<string>(
            "SELECT password_hash FROM users WHERE username = 'admin';");
        Assert.NotEqual("SuperSecret123!", storedHash);
        Assert.True(PasswordHasher.Verify("SuperSecret123!", storedHash));
    }

    [Fact]
    public async Task BootstrapFromOptionsAsync_HandlesEmptyInvalidExistingAndValidOptions()
    {
        var dbPath = Path.Combine(_tempDir, "bootstrap.db");
        var factory = new SqliteConnectionFactory(Options.Create(new DatabaseOptions
        {
            ConnectionString = $"Data Source={dbPath}"
        }));
        var runner = new SqlMigrationRunner(factory, NullLogger<SqlMigrationRunner>.Instance);
        await runner.MigrateAsync();

        // Empty options -> no user created
        await SetupAdminHandler.BootstrapFromOptionsAsync(
            factory,
            Options.Create(new AuthOptions()),
            NullLogger.Instance);

        // Invalid options -> skipped
        await SetupAdminHandler.BootstrapFromOptionsAsync(
            factory,
            Options.Create(new AuthOptions
            {
                InitialAdminUsername = "a",
                InitialAdminPassword = "123"
            }),
            NullLogger.Instance);

        await using (var conn = await factory.OpenConnectionAsync())
        {
            Assert.Equal(0, await conn.QuerySingleAsync<long>("SELECT COUNT(*) FROM users;"));
        }

        // Valid options -> creates admin
        await SetupAdminHandler.BootstrapFromOptionsAsync(
            factory,
            Options.Create(new AuthOptions
            {
                InitialAdminUsername = "envadmin",
                InitialAdminPassword = "EnvPassword123!"
            }),
            NullLogger.Instance);

        // Second call when user already exists -> no-op
        await SetupAdminHandler.BootstrapFromOptionsAsync(
            factory,
            Options.Create(new AuthOptions
            {
                InitialAdminUsername = "otheradmin",
                InitialAdminPassword = "EnvPassword123!"
            }),
            NullLogger.Instance);

        await using (var conn = await factory.OpenConnectionAsync())
        {
            Assert.Equal(1, await conn.QuerySingleAsync<long>("SELECT COUNT(*) FROM users;"));
        }
    }

    private async Task<WebApplication> CreateTestAppAsync(string dbFileName)
    {
        var dbPath = Path.Combine(_tempDir, dbFileName);
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

        var app = builder.Build();
        await app.InitializeDatabaseAsync();
        app.UseAuthorization();
        app.Use(async (context, next) =>
        {
            context.Connection.LocalPort = 18420;
            await next();
        });
        app.MapWebApi(new ServerOptions()).MapSetupAdmin();
        await app.StartAsync();
        return app;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }
}
