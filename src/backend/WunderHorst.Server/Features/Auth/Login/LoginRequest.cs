namespace WunderHorst.Server.Features.Auth.Login;

/// <summary>
/// Request payload for user sign-in.
/// </summary>
public sealed record LoginRequest(string Username, string Password);
