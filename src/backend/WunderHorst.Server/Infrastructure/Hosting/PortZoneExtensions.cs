namespace WunderHorst.Server.Infrastructure.Hosting;

/// <summary>
/// Endpoint filter enforcing that a route can only be reached on its designated Kestrel local port.
/// </summary>
public sealed class PortZoneEndpointFilter(int expectedPort) : IEndpointFilter
{
    private readonly int _expectedPort = expectedPort;

    /// <inheritdoc />
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.HttpContext.Connection.LocalPort != _expectedPort)
        {
            return ValueTask.FromResult<object?>(Results.NotFound());
        }

        return next(context);
    }
}

/// <summary>
/// Route group helpers for binding endpoints to the <c>WebApi</c> or <c>BoxineApi</c> port zone.
/// </summary>
public static class PortZoneExtensions
{
    /// <summary>
    /// Creates a route group restricted to the <c>WebApi</c> local port.
    /// </summary>
    public static RouteGroupBuilder MapWebApi(
        this IEndpointRouteBuilder endpoints,
        ServerOptions options,
        string prefix = "/api")
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(options);

        return endpoints
            .MapGroup(prefix)
            .RequireAuthorization()
            .AddEndpointFilter(new PortZoneEndpointFilter(options.WebApiPort));
    }

    /// <summary>
    /// Creates a route group restricted to the <c>BoxineApi</c> local port.
    /// </summary>
    public static RouteGroupBuilder MapBoxineApi(
        this IEndpointRouteBuilder endpoints,
        ServerOptions options,
        string prefix = "")
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(options);

        return endpoints
            .MapGroup(prefix)
            .AddEndpointFilter(new PortZoneEndpointFilter(options.BoxineApiPort));
    }
}
