namespace WunderHorst.Server.Features.Auth.SetupAdmin;

/// <summary>
/// Validator for <see cref="SetupAdminRequest"/>.
/// </summary>
public static class SetupAdminValidator
{
    public const int MinUsernameLength = 3;
    public const int MaxUsernameLength = 64;
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    /// <summary>
    /// Validates username and password rules and returns field errors if invalid.
    /// </summary>
    public static Dictionary<string, string[]> Validate(SetupAdminRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var trimmedUsername = request.Username?.Trim() ?? string.Empty;

        if (trimmedUsername.Length < MinUsernameLength || trimmedUsername.Length > MaxUsernameLength)
        {
            errors["username"] =
                [$"Username must be between {MinUsernameLength} and {MaxUsernameLength} characters."];
        }

        var password = request.Password ?? string.Empty;
        if (password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
        {
            errors["password"] =
                [$"Password must be between {MinPasswordLength} and {MaxPasswordLength} characters."];
        }

        return errors;
    }
}
