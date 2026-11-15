using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Options;
using WunderHorst.Server.Features.Auth.GetCurrentUser;
using WunderHorst.Server.Features.Auth.Login;
using WunderHorst.Server.Features.Auth.Logout;
using WunderHorst.Server.Features.Auth.SetupAdmin;
using WunderHorst.Server.Features.System.GetHealth;
using WunderHorst.Server.Infrastructure.Auth;
using WunderHorst.Server.Infrastructure.Database;
using WunderHorst.Server.Infrastructure.Errors;
using WunderHorst.Server.Infrastructure.Hosting;
using WunderHorst.Server.Infrastructure.Observability;
using WunderHorst.Server.Infrastructure.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

var serverOptions = new ServerOptions();
builder.Configuration.GetSection(ServerOptions.SectionName).Bind(serverOptions);
builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection(ServerOptions.SectionName));

builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.ListenAnyIP(serverOptions.WebApiPort);
    kestrel.ListenAnyIP(serverOptions.BoxineApiPort);
});

builder.AddObservability();
builder.AddDatabase();
builder.AddCookieAuth();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

var isOpenApiExport = string.Equals(
    Assembly.GetEntryAssembly()?.GetName().Name,
    "GetDocument.Insider",
    StringComparison.Ordinal);

if (!isOpenApiExport)
{
    await app.InitializeDatabaseAsync();
    await SetupAdminHandler.BootstrapFromOptionsAsync(
        app.Services.GetRequiredService<ISqliteConnectionFactory>(),
        app.Services.GetRequiredService<IOptions<AuthOptions>>(),
        app.Logger);
}

app.UseExceptionHandler();
app.UseCookieAuth();

var webApi = app.MapWebApi(serverOptions);
webApi.MapOpenApi("/openapi/{documentName}.json").AllowAnonymous();
webApi.MapGetHealth();
webApi.MapSetupAdmin();
webApi.MapLogin();
webApi.MapLogout();
webApi.MapGetCurrentUser();

_ = app.MapBoxineApi(serverOptions);

app.Run();

/// <summary>
/// Entry point marker for integration test WebApplicationFactory.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class Program;
