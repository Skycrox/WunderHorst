namespace WunderHorst.Server.Features.Auth.GetCurrentUser;

/// <summary>
/// Response payload representing the currently authenticated user.
/// </summary>
public sealed record CurrentUserResponse(string Id, string Username, string Role);
