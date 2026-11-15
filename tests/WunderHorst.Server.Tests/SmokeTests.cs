using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using WunderHorst.Server.Infrastructure.Serialization;

namespace WunderHorst.Server.Tests;

public sealed class SmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void AppJsonSerializerContext_SerializesProblemDetails_AsCamelCase()
    {
        var problem = new ProblemDetails
        {
            Title = "Test Error",
            Status = 400
        };

        var json = JsonSerializer.Serialize(problem, AppJsonSerializerContext.Default.ProblemDetails);

        Assert.Contains("\"title\":\"Test Error\"", json);
        Assert.Contains("\"status\":400", json);
    }

    [Fact]
    public void WebApplicationFactory_CreateClient_Succeeds()
    {
        using var client = _factory.CreateClient();
        Assert.NotNull(client);
    }
}
