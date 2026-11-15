using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WunderHorst.Server.Features.Auth.GetCurrentUser;
using WunderHorst.Server.Features.Auth.Login;
using WunderHorst.Server.Features.Auth.Logout;
using WunderHorst.Server.Features.Auth.SetupAdmin;
using WunderHorst.Server.Infrastructure.Auth;
using WunderHorst.Server.Infrastructure.Database;
using WunderHorst.Server.Infrastructure.Hosting;
using WunderHorst.Server.Infrastructure.Serialization;

namespace WunderHorst.Server.Tests.Features.Auth;

public sealed class AuthFlowTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(
        Path.GetTempPath(),
        "wunderhorst-auth-tests-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void LoginValidator_WhenValid_ReturnsNoErrors()
    {
        var errors = LoginValidator.Validate(new LoginRequest("admin", "secret"));
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("", "secret", "username")]
    [InlineData(null, "secret", "username")]
    [InlineData("admin", "", "password")]
    [InlineData("admin", null, "password")]
    public void LoginValidator_WhenInvalid_ReturnsExpectedErrors(
        string? username,
        string? password,
        string expectedKey)
    {
        var errors = LoginValidator.Validate(new LoginRequest(username!, password!));
        Assert.True(errors.ContainsKey(expectedKey));
    }

    [Fact]
    public void LoginMapperExtensions_MapsUserAndPrincipal()
    {
        var row = new LoginUserRow("id1", "admin", "hash", "Admin");

        var response = row.ToResponse();
        var principal = row.ToClaimsPrincipal();

        Assert.Equal("id1", response.Id);
        Assert.Equal("admin", response.Username);
        Assert.Equal("Admin", response.Role);
        Assert.Equal("id1", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("admin", principal.FindFirstValue(ClaimTypes.Name));
        Assert.Equal("Admin", principal.FindFirstValue(ClaimTypes.Role));
    }

    [Fact]
    public void CurrentUserMapperExtensions_WhenClaimsPresent_ReturnsResponse()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "u1"),
            new Claim(ClaimTypes.Name, "horst"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "Cookies"));

        var result = principal.ToCurrentUserResponse();

        Assert.NotNull(result);
        Assert.Equal("u1", result!.Id);
        Assert.Equal("horst", result.Username);
        Assert.Equal("Admin", result.Role);
    }

    [Theory]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void CurrentUserMapperExtensions_WhenRequiredClaimMissing_ReturnsNull(
        bool hasId,
        bool hasName,
        bool hasRole)
    {
        var claims = new List<Claim>();
        if (hasId) claims.Add(new Claim(ClaimTypes.NameIdentifier, "u1"));
        if (hasName) claims.Add(new Claim(ClaimTypes.Name, "horst"));
        if (hasRole) claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies"));

        Assert.Null(principal.ToCurrentUserResponse());
    }

    [Fact]
    public void GetCurrentUserHandler_WhenUnauthenticated_ReturnsUnauthorized()
    {
        var context = new DefaultHttpContext();
        var result = GetCurrentUserHandler.Handle(context);
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public void GetCurrentUserHandler_WhenAuthenticated_ReturnsOk()
    {
        var context = new DefaultHttpContext
        {
            User = new LoginUserRow("u1", "admin", "hash", "Admin").ToClaimsPrincipal()
        };

        var result = GetCurrentUserHandler.Handle(context);

        var ok = Assert.IsType<Ok<CurrentUserResponse>>(result);
        Assert.Equal("admin", ok.Value!.Username);
    }

    [Fact]
    public async Task AuthFlow_EndToEnd_EnforcesRequireAuthorization_AndCookieSessionLifecycle()
    {
        await using var app = await CreateTestAppAsync("auth-e2e.db");
        using var client = app.GetTestClient();

        // 1. Protected WebApi endpoint without cookie -> 401 Unauthorized
        var unauthMe = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthMe.StatusCode);

        // 2. Setup initial admin
        var setupResp = await client.PostAsJsonAsync(
            "/api/auth/setup",
            new SetupAdminRequest("admin", "SuperSecret123!"),
            AppJsonSerializerContext.Default.SetupAdminRequest);
        Assert.Equal(HttpStatusCode.Created, setupResp.StatusCode);

        // 3. Invalid login payload -> 400
        var invalidLogin = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("", ""),
            AppJsonSerializerContext.Default.LoginRequest);
        Assert.Equal(HttpStatusCode.BadRequest, invalidLogin.StatusCode);

        // 4. Wrong password -> 401
        var wrongPassword = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("admin", "WrongPass!"),
            AppJsonSerializerContext.Default.LoginRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);

        // 5. Unknown user -> 401
        var unknownUser = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("nobody", "SuperSecret123!"),
            AppJsonSerializerContext.Default.LoginRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownUser.StatusCode);

        // 6. Valid login -> 200 OK + Set-Cookie (HttpOnly)
        var loginResp = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("admin", "SuperSecret123!"),
            AppJsonSerializerContext.Default.LoginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

        var setCookies = loginResp.Headers.GetValues("Set-Cookie").ToList();
        var sessionCookie = Assert.Single(setCookies);
        Assert.Contains("wunderhorst_session=", sessionCookie, StringComparison.Ordinal);
        Assert.Contains("httponly", sessionCookie, StringComparison.OrdinalIgnoreCase);

        var cookieValue = sessionCookie.Split(';')[0];

        // 7. GET /api/auth/me with session cookie -> 200 OK
        using var meReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meReq.Headers.Add("Cookie", cookieValue);
        var meResp = await client.SendAsync(meReq);
        Assert.Equal(HttpStatusCode.OK, meResp.StatusCode);

        var me = await meResp.Content.ReadFromJsonAsync(
            AppJsonSerializerContext.Default.CurrentUserResponse);
        Assert.NotNull(me);
        Assert.Equal("admin", me!.Username);
        Assert.Equal("Admin", me.Role);

        // 8. POST /api/auth/logout with session cookie -> 204 NoContent
        using var logoutReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        logoutReq.Headers.Add("Cookie", cookieValue);
        var logoutResp = await client.SendAsync(logoutReq);
        Assert.Equal(HttpStatusCode.NoContent, logoutResp.StatusCode);

        // 9. GET /api/auth/me on BoxineApi port -> 404 NotFound
        using var boxineReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        boxineReq.Headers.Add("X-Simulated-LocalPort", "18443");
        boxineReq.Headers.Add("Cookie", cookieValue);
        var boxineResp = await client.SendAsync(boxineReq);
        Assert.Equal(HttpStatusCode.NotFound, boxineResp.StatusCode);
    }

    private async Task<WebApplication> CreateTestAppAsync(string dbFileName)
    {
        var dbPath = Path.Combine(_tempDir, dbFileName);
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = $"Data Source={dbPath}",
            ["Auth:RequireSecureCookie"] = "false"
        });
        builder.AddDatabase();
        builder.AddCookieAuth();
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        var app = builder.Build();
        await app.InitializeDatabaseAsync();
        app.Use(async (context, next) =>
        {
            context.Connection.LocalPort = context.Request.Headers.TryGetValue("X-Simulated-LocalPort", out var p)
                && int.TryParse(p, out var parsed)
                ? parsed
                : 18420;
            await next();
        });
        app.UseCookieAuth();

        var webApi = app.MapWebApi(new ServerOptions());
        webApi.MapSetupAdmin();
        webApi.MapLogin();
        webApi.MapLogout();
        webApi.MapGetCurrentUser();

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
