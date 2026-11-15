namespace WunderHorst.Server.Features.System.GetHealth;

/// <summary>
/// Endpoint registration for <c>GET /api/health</c>.
/// </summary>
public static class GetHealthEndpoint
{
    /// <summary>
    /// Maps the anonymous health check endpoint onto the <c>WebApi</c> route group.
    /// </summary>
    public static RouteGroupBuilder MapGetHealth(this RouteGroupBuilder webApiGroup)
    {
        webApiGroup
            .MapGet("/health", GetHealthHandler.HandleAsync)
            .AllowAnonymous()
            .WithName("GetHealth")
            .Produces<GetHealthResponse>(StatusCodes.Status200OK)
            .Produces<GetHealthResponse>(StatusCodes.Status503ServiceUnavailable);

        return webApiGroup;
    }
}
