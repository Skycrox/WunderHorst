using System.Diagnostics.CodeAnalysis;
using WunderHorst.Server.Infrastructure.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.Run();

/// <summary>
/// Entry point marker for integration test WebApplicationFactory.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class Program;
