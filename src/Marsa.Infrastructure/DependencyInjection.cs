using Marsa.Infrastructure.Docker;
using Marsa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Marsa.Infrastructure;

/// <summary>
/// Dependency injection registrations for Infrastructure adapters.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddMarsaInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<MarsaDbContext>(options =>
            {
                options.UseNpgsql(connectionString)
                       .UseSnakeCaseNamingConvention();
            });
        }

        services.AddSingleton<IDockerCapabilityProbe, DockerCapabilityProbe>();

        return services;
    }
}
