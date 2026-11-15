using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
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
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

await app.InitializeDatabaseAsync();
await SetupAdminHandler.BootstrapFromOptionsAsync(
    app.Services.GetRequiredService<ISqliteConnectionFactory>(),
    app.Services.GetRequiredService<IOptions<AuthOptions>>(),
    app.Logger);

app.UseExceptionHandler();

var webApi = app.MapWebApi(serverOptions);
webApi.MapGetHealth();
webApi.MapSetupAdmin();

_ = app.MapBoxineApi(serverOptions);

app.Run();

/// <summary>
/// Entry point marker for integration test WebApplicationFactory.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class Program;
