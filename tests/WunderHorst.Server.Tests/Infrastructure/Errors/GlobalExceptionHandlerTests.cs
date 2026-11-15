using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WunderHorst.Server.Infrastructure.Errors;

namespace WunderHorst.Server.Tests.Infrastructure.Errors;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenExceptionThrown_WritesProblemDetailsAndReturnsTrue()
    {
        var problemDetailsService = new FakeProblemDetailsService(writeResult: true);
        var handler = new GlobalExceptionHandler(problemDetailsService, NullLogger<GlobalExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/boom";

        var handled = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("Boom"),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        Assert.NotNull(problemDetailsService.LastContext);
        Assert.Equal(500, problemDetailsService.LastContext!.ProblemDetails.Status);
        Assert.Equal("An unexpected error occurred.", problemDetailsService.LastContext.ProblemDetails.Title);
        Assert.Equal("/api/boom", problemDetailsService.LastContext.ProblemDetails.Instance);
    }

    [Fact]
    public async Task ExceptionHandlerMiddleware_WhenEndpointThrows_ReturnsRfc9457ProblemDetails()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        await using var app = builder.Build();
        app.UseExceptionHandler();
        app.MapGet("/boom", () =>
        {
            throw new InvalidOperationException("Unhandled test failure");
        });

        await app.StartAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/boom");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(500, problem!.Status);
        Assert.Equal("An unexpected error occurred.", problem.Title);
    }

    private sealed class FakeProblemDetailsService(bool writeResult) : IProblemDetailsService
    {
        public ProblemDetailsContext? LastContext { get; private set; }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            LastContext = context;
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            LastContext = context;
            return ValueTask.FromResult(writeResult);
        }
    }
}
