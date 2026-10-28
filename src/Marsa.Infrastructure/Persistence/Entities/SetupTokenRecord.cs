namespace Marsa.Infrastructure.Persistence.Entities;

public sealed class SetupTokenRecord
{
    public Guid Id { get; set; }
    public byte[] TokenHash { get; set; } = Array.Empty<byte>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
}
