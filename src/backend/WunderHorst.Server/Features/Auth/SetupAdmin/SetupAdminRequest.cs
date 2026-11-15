namespace WunderHorst.Server.Features.Auth.SetupAdmin;

/// <summary>
/// Request payload for creating the initial administrator account.
/// </summary>
public sealed record SetupAdminRequest(string Username, string Password);
