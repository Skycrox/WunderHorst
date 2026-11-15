using System.Diagnostics.CodeAnalysis;
using WunderHorst.Server.Infrastructure.Hosting;
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

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

_ = app.MapWebApi(serverOptions);
_ = app.MapBoxineApi(serverOptions);

app.Run();

/// <summary>
/// Entry point marker for integration test WebApplicationFactory.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class Program;
