namespace AselDevBlazorArchitecture.Infrastructure.Data;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool Enabled { get; set; }
    public string Provider { get; set; } = "mysql";
    public string ConnectionString { get; set; } = string.Empty;
}
