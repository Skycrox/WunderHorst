namespace WunderHorst.Server.Features.Auth.SetupAdmin;

/// <summary>
/// Endpoint registration for initial administrator setup.
/// </summary>
public static class SetupAdminEndpoint
{
    /// <summary>
    /// Maps <c>GET /api/auth/setup-status</c> and <c>POST /api/auth/setup</c>.
    /// </summary>
    public static RouteGroupBuilder MapSetupAdmin(this RouteGroupBuilder webApiGroup)
    {
        webApiGroup
            .MapGet("/auth/setup-status", SetupAdminHandler.GetStatusAsync)
            .AllowAnonymous()
            .WithName("GetSetupStatus")
            .Produces<SetupStatusResponse>(StatusCodes.Status200OK);

        webApiGroup
            .MapPost("/auth/setup", SetupAdminHandler.HandleAsync)
            .AllowAnonymous()
            .WithName("SetupAdmin")
            .Produces<SetupAdminResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        return webApiGroup;
    }
}
