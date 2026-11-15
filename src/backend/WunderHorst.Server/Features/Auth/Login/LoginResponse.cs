namespace WunderHorst.Server.Features.Auth.Login;

/// <summary>
/// Response payload returned after a successful sign-in.
/// </summary>
public sealed record LoginResponse(string Id, string Username, string Role);
