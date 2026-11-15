using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WunderHorst.Server.Features.System.GetHealth;
using WunderHorst.Server.Infrastructure.Hosting;
using WunderHorst.Server.Infrastructure.Serialization;

namespace WunderHorst.Server.Tests.Features.System;

public sealed class GetHealthTests
{
    [Fact]
    public void ToResponse_WhenReportHasEntries_MapsStatusAndChecks()
    {
        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["sqlite"] = new(HealthStatus.Healthy, "ok", TimeSpan.FromMilliseconds(2), null, null)
        };
        var report = new HealthReport(entries, TimeSpan.FromMilliseconds(2));

        var response = report.ToResponse();

        Assert.Equal("Healthy", response.Status);
        Assert.Single(response.Checks);
        Assert.Equal("Healthy", response.Checks["sqlite"]);
    }

    [Fact]
    public async Task HandleAsync_WhenHealthy_ReturnsOk200()
    {
        var service = new FakeHealthCheckService(HealthStatus.Healthy);

        var result = await GetHealthHandler.HandleAsync(service, CancellationToken.None);

        var okResult = Assert.IsType<Ok<GetHealthResponse>>(result);
        Assert.Equal("Healthy", okResult.Value!.Status);
    }

    [Fact]
    public async Task HandleAsync_WhenUnhealthy_ReturnsServiceUnavailable503()
    {
        var service = new FakeHealthCheckService(HealthStatus.Unhealthy);

        var result = await GetHealthHandler.HandleAsync(service, CancellationToken.None);

        var jsonResult = Assert.IsType<JsonHttpResult<GetHealthResponse>>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, jsonResult.StatusCode);
        Assert.Equal("Unhealthy", jsonResult.Value!.Status);
    }

    [Fact]
    public async Task GetHealthEndpoint_WhenCalledOnWebApiPort_Returns200_AndRejectsBoxinePort()
    {
        var serverOptions = new ServerOptions
        {
            WebApiPort = 18420,
            BoxineApiPort = 18443
        };

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddHealthChecks();
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        await using var app = builder.Build();
        app.Use(async (context, next) =>
        {
            if (context.Request.Headers.TryGetValue("X-Simulated-LocalPort", out var portHeader) &&
                int.TryParse(portHeader, out var port))
            {
                context.Connection.LocalPort = port;
            }

            await next();
        });

        app.MapWebApi(serverOptions).MapGetHealth();
        await app.StartAsync();

        using var client = app.GetTestClient();

        using var webApiRequest = new HttpRequestMessage(HttpMethod.Get, "/api/health");
        webApiRequest.Headers.Add("X-Simulated-LocalPort", "18420");
        var webApiResponse = await client.SendAsync(webApiRequest);

        Assert.Equal(HttpStatusCode.OK, webApiResponse.StatusCode);
        var payload = await webApiResponse.Content.ReadFromJsonAsync(
            AppJsonSerializerContext.Default.GetHealthResponse);
        Assert.NotNull(payload);
        Assert.Equal("Healthy", payload!.Status);

        using var boxineRequest = new HttpRequestMessage(HttpMethod.Get, "/api/health");
        boxineRequest.Headers.Add("X-Simulated-LocalPort", "18443");
        var boxineResponse = await client.SendAsync(boxineRequest);

        Assert.Equal(HttpStatusCode.NotFound, boxineResponse.StatusCode);
    }

    private sealed class FakeHealthCheckService(HealthStatus status) : HealthCheckService
    {
        public override Task<HealthReport> CheckHealthAsync(
            Func<HealthCheckRegistration, bool>? predicate,
            CancellationToken cancellationToken = default)
        {
            var entries = new Dictionary<string, HealthReportEntry>
            {
                ["test"] = new(status, null, TimeSpan.Zero, null, null)
            };
            return Task.FromResult(new HealthReport(entries, TimeSpan.Zero));
        }
    }
}
