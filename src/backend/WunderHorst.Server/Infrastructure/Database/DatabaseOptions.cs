namespace WunderHorst.Server.Infrastructure.Database;

/// <summary>
/// AOT-bound configuration options for SQLite persistence.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// SQLite connection string (e.g. <c>Data Source=data/wunderhorst.db</c>).
    /// </summary>
    public string ConnectionString { get; set; } = "Data Source=data/wunderhorst.db";
}
