using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WunderHorst.Server.Infrastructure.Auth;
using WunderHorst.Server.Infrastructure.Database;

namespace WunderHorst.Server.Features.Auth.Login;

/// <summary>
/// Database projection for credential verification.
/// </summary>
public sealed record LoginUserRow(string Id, string Username, string PasswordHash, string Role);

/// <summary>
/// Mapping extensions for <see cref="LoginUserRow"/>.
/// </summary>
public static class LoginMapperExtensions
{
    /// <summary>
    /// Maps a <see cref="LoginUserRow"/> to a <see cref="LoginResponse"/>.
    /// </summary>
    public static LoginResponse ToResponse(this LoginUserRow user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new LoginResponse(user.Id, user.Username, user.Role);
    }

    /// <summary>
    /// Creates a cookie <see cref="ClaimsPrincipal"/> for the authenticated user.
    /// </summary>
    public static ClaimsPrincipal ToClaimsPrincipal(this LoginUserRow user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}

/// <summary>
/// Handler for user sign-in via HttpOnly session cookie.
/// </summary>
public static class LoginHandler
{
    /// <summary>
    /// Verifies user credentials and issues an authenticated session cookie.
    /// </summary>
    public static async Task<IResult> HandleAsync(
        LoginRequest request,
        HttpContext httpContext,
        ISqliteConnectionFactory connectionFactory,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var errors = LoginValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var user = await connection.QuerySingleOrDefaultAsync<LoginUserRow>(
            """
            SELECT id AS Id, username AS Username, password_hash AS PasswordHash, role AS Role
            FROM users
            WHERE username = @Username
            LIMIT 1;
            """,
            new { Username = request.Username.Trim() });

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Results.Problem(
                title: "Invalid credentials.",
                detail: "Username or password is incorrect.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var principal = user.ToClaimsPrincipal();
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        var logger = loggerFactory.CreateLogger(typeof(LoginHandler).FullName!);
        logger.LogInformation("User {Username} signed in", user.Username);

        return Results.Ok(user.ToResponse());
    }
}
