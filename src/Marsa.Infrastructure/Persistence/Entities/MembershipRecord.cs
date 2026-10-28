namespace Marsa.Infrastructure.Persistence.Entities;

public sealed class MembershipRecord
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTimeOffset? ArchivedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public uint Xmin { get; set; }

    public OrganizationRecord? Organization { get; set; }
    public UserRecord? User { get; set; }
}
