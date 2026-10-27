using Marsa.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marsa.Infrastructure.Persistence;

/// <summary>
/// Primary Entity Framework Core database context for Marsa.
/// </summary>
public sealed class MarsaDbContext : DbContext
{
    public MarsaDbContext(DbContextOptions<MarsaDbContext> options) : base(options)
    {
    }

    public DbSet<OrganizationRecord> Organizations => Set<OrganizationRecord>();
    public DbSet<SchemaVersionRecord> SchemaVersions => Set<SchemaVersionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrganizationRecord>(entity =>
        {
            entity.ToTable("organizations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Slug).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.Xmin).IsRowVersion();
        });

        modelBuilder.Entity<SchemaVersionRecord>(entity =>
        {
            entity.ToTable("schema_versions");
            entity.HasKey(e => e.Version);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.AppliedAt).IsRequired();
        });
    }
}
