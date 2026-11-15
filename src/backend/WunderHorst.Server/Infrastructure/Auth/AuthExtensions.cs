using Microsoft.AspNetCore.Authentication.Cookies;

namespace WunderHorst.Server.Infrastructure.Auth;

/// <summary>
/// Dependency injection and middleware extensions for HttpOnly cookie authentication.
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Registers cookie authentication and authorization policies for <c>WebApi</c>.
    /// </summary>
    public static WebApplicationBuilder AddCookieAuth(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var authOptions = new AuthOptions();
        builder.Configuration.GetSection(AuthOptions.SectionName).Bind(authOptions);
        builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));

        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = authOptions.CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = authOptions.RequireSecureCookie
                    ? CookieSecurePolicy.Always
                    : CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        builder.Services.AddAuthorization();

        return builder;
    }

    /// <summary>
    /// Enables authentication and authorization middleware.
    /// </summary>
    public static WebApplication UseCookieAuth(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
