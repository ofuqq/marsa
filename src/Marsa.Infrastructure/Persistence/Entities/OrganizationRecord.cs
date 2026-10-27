namespace Marsa.Infrastructure.Persistence.Entities;

/// <summary>
/// Persistence record for an organization tenant.
/// </summary>
public sealed class OrganizationRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTimeOffset? SetupCompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public uint Xmin { get; set; }
}
