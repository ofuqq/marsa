using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Marsa.Infrastructure.Persistence;

/// <summary>
/// Design-time factory enabling dotnet-ef migrations and bundle creation.
/// </summary>
public sealed class MarsaDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MarsaDbContext>
{
    public MarsaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MarsaDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=marsa;Username=marsa;Password=marsa")
                      .UseSnakeCaseNamingConvention();

        return new MarsaDbContext(optionsBuilder.Options);
    }
}
