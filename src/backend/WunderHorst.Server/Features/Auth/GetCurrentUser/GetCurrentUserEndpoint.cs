using System.Security.Claims;

namespace WunderHorst.Server.Features.Auth.GetCurrentUser;

/// <summary>
/// Mapping extensions for <see cref="ClaimsPrincipal"/>.
/// </summary>
public static class CurrentUserMapperExtensions
{
    /// <summary>
    /// Maps an authenticated <see cref="ClaimsPrincipal"/> to a <see cref="CurrentUserResponse"/>,
    /// or returns <c>null</c> if required claims are missing.
    /// </summary>
    public static CurrentUserResponse? ToCurrentUserResponse(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = principal.FindFirstValue(ClaimTypes.Name);
        var role = principal.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(id) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        return new CurrentUserResponse(id, username, role);
    }
}

/// <summary>
/// Handler for retrieving the currently authenticated user's profile.
/// </summary>
public static class GetCurrentUserHandler
{
    /// <summary>
    /// Returns the authenticated user's claims or <c>401 Unauthorized</c>.
    /// </summary>
    public static IResult Handle(HttpContext httpContext)
    {
        var response = httpContext.User.ToCurrentUserResponse();
        return response is null
            ? Results.Unauthorized()
            : Results.Ok(response);
    }
}

/// <summary>
/// Endpoint registration for <c>GET /api/auth/me</c>.
/// </summary>
public static class GetCurrentUserEndpoint
{
    /// <summary>
    /// Maps the authenticated current-user endpoint onto the <c>WebApi</c> route group.
    /// </summary>
    public static RouteGroupBuilder MapGetCurrentUser(this RouteGroupBuilder webApiGroup)
    {
        webApiGroup
            .MapGet("/auth/me", GetCurrentUserHandler.Handle)
            .WithName("GetCurrentUser")
            .Produces<CurrentUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return webApiGroup;
    }
}
