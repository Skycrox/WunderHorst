using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using WunderHorst.Server.Infrastructure.Hosting;

namespace WunderHorst.Server.Tests.Infrastructure.Hosting;

public sealed class PortZoneTests
{
    [Fact]
    public async Task InvokeAsync_WhenLocalPortMatchesExpected_CallsNextDelegate()
    {
        var filter = new PortZoneEndpointFilter(18420);
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.LocalPort = 18420;
        var invocationContext = new DefaultEndpointFilterInvocationContext(httpContext);
        var nextCalled = false;

        var result = await filter.InvokeAsync(
            invocationContext,
            _ =>
            {
                nextCalled = true;
                return ValueTask.FromResult<object?>("ok");
            });

        Assert.True(nextCalled);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task InvokeAsync_WhenLocalPortDoesNotMatchExpected_ReturnsNotFound()
    {
        var filter = new PortZoneEndpointFilter(18420);
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.LocalPort = 18443;
        var invocationContext = new DefaultEndpointFilterInvocationContext(httpContext);
        var nextCalled = false;

        var result = await filter.InvokeAsync(
            invocationContext,
            _ =>
            {
                nextCalled = true;
                return ValueTask.FromResult<object?>("ok");
            });

        Assert.False(nextCalled);
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task PortZoneRouting_EnforcesStrictPortIsolationBetweenWebApiAndBoxineApi()
    {
        var serverOptions = new ServerOptions
        {
            WebApiPort = 18420,
            BoxineApiPort = 18443
        };

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddAuthorization();
        await using var app = builder.Build();
        app.UseAuthorization();

        app.Use(async (context, next) =>
        {
            if (context.Request.Headers.TryGetValue("X-Simulated-LocalPort", out var portHeader) &&
                int.TryParse(portHeader, out var port))
            {
                context.Connection.LocalPort = port;
            }

            await next();
        });

        app.MapWebApi(serverOptions).MapGet("/ping", () => Results.Ok("webapi")).AllowAnonymous();
        app.MapBoxineApi(serverOptions).MapGet("/v1/ping", () => Results.Ok("boxine"));

        await app.StartAsync();
        using var client = app.GetTestClient();

        // WebApi route on WebApi port -> 200 OK
        using var webOnWebReq = new HttpRequestMessage(HttpMethod.Get, "/api/ping");
        webOnWebReq.Headers.Add("X-Simulated-LocalPort", "18420");
        var webOnWebResp = await client.SendAsync(webOnWebReq);
        Assert.Equal(HttpStatusCode.OK, webOnWebResp.StatusCode);

        // WebApi route on BoxineApi port -> 404 NotFound
        using var webOnBoxineReq = new HttpRequestMessage(HttpMethod.Get, "/api/ping");
        webOnBoxineReq.Headers.Add("X-Simulated-LocalPort", "18443");
        var webOnBoxineResp = await client.SendAsync(webOnBoxineReq);
        Assert.Equal(HttpStatusCode.NotFound, webOnBoxineResp.StatusCode);

        // BoxineApi route on BoxineApi port -> 200 OK
        using var boxineOnBoxineReq = new HttpRequestMessage(HttpMethod.Get, "/v1/ping");
        boxineOnBoxineReq.Headers.Add("X-Simulated-LocalPort", "18443");
        var boxineOnBoxineResp = await client.SendAsync(boxineOnBoxineReq);
        Assert.Equal(HttpStatusCode.OK, boxineOnBoxineResp.StatusCode);

        // BoxineApi route on WebApi port -> 404 NotFound
        using var boxineOnWebReq = new HttpRequestMessage(HttpMethod.Get, "/v1/ping");
        boxineOnWebReq.Headers.Add("X-Simulated-LocalPort", "18420");
        var boxineOnWebResp = await client.SendAsync(boxineOnWebReq);
        Assert.Equal(HttpStatusCode.NotFound, boxineOnWebResp.StatusCode);
    }
}
