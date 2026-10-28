using Marsa.Infrastructure.Persistence;
using Marsa.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Marsa.Infrastructure.Tests;

public sealed class MigrationTests
{
    [Fact]
    public void ModelConfiguresOrganizationsTableWithExpectedInvariants()
    {
        var factory = new MarsaDesignTimeDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());

        var entityType = context.Model.FindEntityType(typeof(OrganizationRecord));
        Assert.NotNull(entityType);
        Assert.Equal("organizations", entityType.GetTableName());

        var nameProp = entityType.FindProperty(nameof(OrganizationRecord.Name));
        Assert.NotNull(nameProp);
        Assert.False(nameProp.IsNullable);
        Assert.Equal(120, nameProp.GetMaxLength());

        var slugProp = entityType.FindProperty(nameof(OrganizationRecord.Slug));
        Assert.NotNull(slugProp);
        Assert.False(slugProp.IsNullable);

        var slugIndex = entityType.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(OrganizationRecord.Slug)));
        Assert.NotNull(slugIndex);
        Assert.True(slugIndex.IsUnique);

        var xminProp = entityType.FindProperty(nameof(OrganizationRecord.Xmin));
        Assert.NotNull(xminProp);
        Assert.True(xminProp.IsConcurrencyToken);
    }

    [Fact]
    public void ModelConfiguresSchemaVersionsTable()
    {
        var factory = new MarsaDesignTimeDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());

        var entityType = context.Model.FindEntityType(typeof(SchemaVersionRecord));
        Assert.NotNull(entityType);
        Assert.Equal("schema_versions", entityType.GetTableName());

        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Equal(nameof(SchemaVersionRecord.Version), primaryKey.Properties.Single().Name);
    }

    [Fact]
    public void ModelConfiguresUsersTableWithUniqueNormalizedEmail()
    {
        var factory = new MarsaDesignTimeDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());

        var entityType = context.Model.FindEntityType(typeof(UserRecord));
        Assert.NotNull(entityType);
        Assert.Equal("users", entityType.GetTableName());

        var emailProp = entityType.FindProperty(nameof(UserRecord.Email));
        Assert.NotNull(emailProp);
        Assert.False(emailProp.IsNullable);

        var normalizedEmailIndex = entityType.GetIndexes()
            .SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(UserRecord.NormalizedEmail)));
        Assert.NotNull(normalizedEmailIndex);
        Assert.True(normalizedEmailIndex.IsUnique);
    }

    [Fact]
    public void ModelConfiguresMembershipsTable()
    {
        var factory = new MarsaDesignTimeDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());

        var entityType = context.Model.FindEntityType(typeof(MembershipRecord));
        Assert.NotNull(entityType);
        Assert.Equal("memberships", entityType.GetTableName());

        var orgUserIndex = entityType.GetIndexes().SingleOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == nameof(MembershipRecord.OrganizationId)) &&
            i.Properties.Any(p => p.Name == nameof(MembershipRecord.UserId)));
        Assert.NotNull(orgUserIndex);
        Assert.True(orgUserIndex.IsUnique);
    }

    [Fact]
    public void ModelConfiguresSetupTokensTableWithUniqueHash()
    {
        var factory = new MarsaDesignTimeDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());

        var entityType = context.Model.FindEntityType(typeof(SetupTokenRecord));
        Assert.NotNull(entityType);
        Assert.Equal("setup_tokens", entityType.GetTableName());

        var hashIndex = entityType.GetIndexes()
            .SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(SetupTokenRecord.TokenHash)));
        Assert.NotNull(hashIndex);
        Assert.True(hashIndex.IsUnique);
    }

    [Fact]
    public void ModelConfiguresSystemSetupSingletonConstraint()
    {
        var factory = new MarsaDesignTimeDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());

        var designTimeModel = Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions.GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>(context).Model;
        var entityType = designTimeModel.FindEntityType(typeof(SystemSetupRecord));
        Assert.NotNull(entityType);
        Assert.Equal("system_setup", entityType.GetTableName());

        var checkConstraint = entityType.GetCheckConstraints()
            .SingleOrDefault(c => c.Name == "CK_system_setup_singleton");
        Assert.NotNull(checkConstraint);
        Assert.Equal("id = 1", checkConstraint.Sql);
    }

    [Fact]
    public void DesignTimeFactoryProducesValidContext()
    {
        var factory = new MarsaDesignTimeDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());

        Assert.NotNull(context);
        Assert.NotNull(context.Database);
    }
}
