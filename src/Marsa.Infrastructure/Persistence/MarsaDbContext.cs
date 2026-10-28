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
    public DbSet<UserRecord> Users => Set<UserRecord>();
    public DbSet<MembershipRecord> Memberships => Set<MembershipRecord>();
    public DbSet<SetupTokenRecord> SetupTokens => Set<SetupTokenRecord>();
    public DbSet<SystemSetupRecord> SystemSetup => Set<SystemSetupRecord>();
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

        modelBuilder.Entity<UserRecord>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.NormalizedEmail).IsRequired();
            entity.HasIndex(e => e.NormalizedEmail).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(e => e.EmailConfirmed).IsRequired();
            entity.Property(e => e.SecurityStamp).IsRequired();
            entity.Property(e => e.ConcurrencyStamp).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.Xmin).IsRowVersion();
        });

        modelBuilder.Entity<MembershipRecord>(entity =>
        {
            entity.ToTable("memberships");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.Xmin).IsRowVersion();

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.OrganizationId, e.UserId }).IsUnique();
        });

        modelBuilder.Entity<SetupTokenRecord>(entity =>
        {
            entity.ToTable("setup_tokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TokenHash).IsRequired();
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
        });

        modelBuilder.Entity<SystemSetupRecord>(entity =>
        {
            entity.ToTable("system_setup", t =>
            {
                t.HasCheckConstraint("CK_system_setup_singleton", "id = 1");
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IsCompleted).IsRequired();
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
