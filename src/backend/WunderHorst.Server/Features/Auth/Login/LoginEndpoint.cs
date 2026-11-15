namespace WunderHorst.Server.Features.Auth.Login;

/// <summary>
/// Endpoint registration for <c>POST /api/auth/login</c>.
/// </summary>
public static class LoginEndpoint
{
    /// <summary>
    /// Maps the anonymous login endpoint onto the <c>WebApi</c> route group.
    /// </summary>
    public static RouteGroupBuilder MapLogin(this RouteGroupBuilder webApiGroup)
    {
        webApiGroup
            .MapPost("/auth/login", LoginHandler.HandleAsync)
            .AllowAnonymous()
            .WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return webApiGroup;
    }
}
