namespace WunderHorst.Server.Infrastructure.Auth;

/// <summary>
/// AOT-bound authentication and session cookie options.
/// </summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>
    /// Cookie name for the HttpOnly session cookie.
    /// </summary>
    public string CookieName { get; set; } = "wunderhorst_session";

    /// <summary>
    /// Controls the <c>Secure</c> attribute on session cookies (can be disabled for LAN HTTP setups).
    /// </summary>
    public bool RequireSecureCookie { get; set; } = true;

    /// <summary>
    /// Optional initial admin username for headless bootstrap via environment variables.
    /// </summary>
    public string? InitialAdminUsername { get; set; }

    /// <summary>
    /// Optional initial admin password for headless bootstrap via environment variables.
    /// </summary>
    public string? InitialAdminPassword { get; set; }
}
