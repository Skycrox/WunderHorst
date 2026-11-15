namespace WunderHorst.Server.Features.Auth.SetupAdmin;

/// <summary>
/// Response payload returned after creating the initial administrator account.
/// </summary>
public sealed record SetupAdminResponse(string Id, string Username, string Role, string CreatedAt);

/// <summary>
/// Response payload indicating whether initial admin setup is still required.
/// </summary>
public sealed record SetupStatusResponse(bool NeedsSetup);
