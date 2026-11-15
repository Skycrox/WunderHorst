namespace WunderHorst.Server.Features.System.GetHealth;

/// <summary>
/// Response payload for the <c>/api/health</c> endpoint.
/// </summary>
public sealed record GetHealthResponse(
    string Status,
    IReadOnlyDictionary<string, string> Checks);
