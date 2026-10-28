namespace Marsa.Infrastructure.Persistence.Entities;

public sealed class SystemSetupRecord
{
    public int Id { get; set; } = 1;
    public bool IsCompleted { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? AdministratorId { get; set; }
}
