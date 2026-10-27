namespace Marsa.Infrastructure.Persistence.Entities;

/// <summary>
/// Persistence record for tracking applied database schema versions.
/// </summary>
public sealed class SchemaVersionRecord
{
    public int Version { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset AppliedAt { get; set; }
}
