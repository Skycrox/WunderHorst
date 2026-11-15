namespace WunderHorst.Server.Infrastructure.Hosting;

/// <summary>
/// Configuration options for the dual-port Kestrel server zones.
/// </summary>
public sealed class ServerOptions
{
    public const string SectionName = "Server";

    /// <summary>
    /// Port for the Web-UI and admin API (<c>WebApi</c> zone).
    /// </summary>
    public int WebApiPort { get; set; } = 18420;

    /// <summary>
    /// Port for the Toniebox protocol endpoints (<c>BoxineApi</c> zone).
    /// </summary>
    public int BoxineApiPort { get; set; } = 18443;
}
