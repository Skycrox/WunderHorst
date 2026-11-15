namespace WunderHorst.Server.Features.Auth.Login;

/// <summary>
/// Validator for <see cref="LoginRequest"/>.
/// </summary>
public static class LoginValidator
{
    /// <summary>
    /// Validates that username and password are provided.
    /// </summary>
    public static Dictionary<string, string[]> Validate(LoginRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            errors["username"] = ["Username is required."];
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            errors["password"] = ["Password is required."];
        }

        return errors;
    }
}
