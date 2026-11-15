using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WunderHorst.Server.Features.Auth.Logout;

/// <summary>
/// Handler for terminating the current authenticated session.
/// </summary>
public static class LogoutHandler
{
    /// <summary>
    /// Clears the HttpOnly session cookie.
    /// </summary>
    public static async Task<IResult> HandleAsync(HttpContext httpContext, CancellationToken _)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.NoContent();
    }
}

/// <summary>
/// Endpoint registration for <c>POST /api/auth/logout</c>.
/// </summary>
public static class LogoutEndpoint
{
    /// <summary>
    /// Maps the authenticated logout endpoint onto the <c>WebApi</c> route group.
    /// </summary>
    public static RouteGroupBuilder MapLogout(this RouteGroupBuilder webApiGroup)
    {
        webApiGroup
            .MapPost("/auth/logout", LogoutHandler.HandleAsync)
            .WithName("Logout")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

        return webApiGroup;
    }
}
