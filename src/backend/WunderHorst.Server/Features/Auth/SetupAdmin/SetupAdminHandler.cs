using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using WunderHorst.Server.Infrastructure.Auth;
using WunderHorst.Server.Infrastructure.Database;

namespace WunderHorst.Server.Features.Auth.SetupAdmin;

/// <summary>
/// Database row representation for a user record.
/// </summary>
public sealed record UserRow(string Id, string Username, string Role, string CreatedAt);

/// <summary>
/// Mapping extensions for <see cref="UserRow"/>.
/// </summary>
public static class SetupAdminMapperExtensions
{
    /// <summary>
    /// Maps a <see cref="UserRow"/> to a <see cref="SetupAdminResponse"/>.
    /// </summary>
    public static SetupAdminResponse ToResponse(this UserRow user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new SetupAdminResponse(user.Id, user.Username, user.Role, user.CreatedAt);
    }
}

/// <summary>
/// Handler for initial administrator account creation and setup status queries.
/// </summary>
public static class SetupAdminHandler
{
    public const string AdminRole = "Admin";

    /// <summary>
    /// Returns whether initial admin setup is still required.
    /// </summary>
    public static async Task<IResult> GetStatusAsync(
        ISqliteConnectionFactory connectionFactory,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var userCount = await connection.QuerySingleAsync<long>("SELECT COUNT(*) FROM users;");
        return Results.Ok(new SetupStatusResponse(userCount == 0));
    }

    /// <summary>
    /// Creates the initial administrator account if no user exists yet.
    /// </summary>
    public static async Task<IResult> HandleAsync(
        SetupAdminRequest request,
        ISqliteConnectionFactory connectionFactory,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var errors = SetupAdminValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var userCount = await connection.QuerySingleAsync<long>("SELECT COUNT(*) FROM users;");
        if (userCount > 0)
        {
            return Results.Problem(
                title: "Initial setup already completed.",
                detail: "An administrator account already exists.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var user = await InsertAdminAsync(connection, request.Username, request.Password);
        var logger = loggerFactory.CreateLogger(typeof(SetupAdminHandler).FullName!);
        logger.LogInformation("Initial administrator account {Username} created", user.Username);

        return Results.Created("/api/auth/me", user.ToResponse());
    }

    /// <summary>
    /// Optionally creates the initial admin account from <see cref="AuthOptions"/> on startup.
    /// </summary>
    public static async Task BootstrapFromOptionsAsync(
        ISqliteConnectionFactory connectionFactory,
        IOptions<AuthOptions> authOptions,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var options = authOptions.Value;
        if (string.IsNullOrWhiteSpace(options.InitialAdminUsername) ||
            string.IsNullOrWhiteSpace(options.InitialAdminPassword))
        {
            return;
        }

        var request = new SetupAdminRequest(options.InitialAdminUsername, options.InitialAdminPassword);
        var errors = SetupAdminValidator.Validate(request);
        if (errors.Count > 0)
        {
            logger.LogWarning("Skipping initial admin bootstrap due to invalid Auth:InitialAdmin* configuration.");
            return;
        }

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var userCount = await connection.QuerySingleAsync<long>("SELECT COUNT(*) FROM users;");
        if (userCount > 0)
        {
            return;
        }

        var user = await InsertAdminAsync(connection, request.Username, request.Password);
        logger.LogInformation("Bootstrapped initial administrator account {Username} from configuration", user.Username);
    }

    private static async Task<UserRow> InsertAdminAsync(
        SqliteConnection connection,
        string username,
        string password)
    {
        var user = new UserRow(
            Id: Guid.NewGuid().ToString("N"),
            Username: username.Trim(),
            Role: AdminRole,
            CreatedAt: DateTimeOffset.UtcNow.ToString("O"));

        var passwordHash = PasswordHasher.Hash(password);

        await connection.ExecuteAsync(
            """
            INSERT INTO users (id, username, password_hash, role, created_at)
            VALUES (@Id, @Username, @PasswordHash, @Role, @CreatedAt);
            """,
            new
            {
                user.Id,
                user.Username,
                PasswordHash = passwordHash,
                user.Role,
                user.CreatedAt
            });

        return user;
    }
}
