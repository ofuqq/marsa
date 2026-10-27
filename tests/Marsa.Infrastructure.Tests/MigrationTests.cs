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
    public void DesignTimeFactoryProducesValidContext()
    {
        var factory = new MarsaDesignTimeDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());

        Assert.NotNull(context);
        Assert.NotNull(context.Database);
    }
}
