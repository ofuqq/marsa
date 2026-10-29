using Marsa.Application.Authentication;
using Marsa.Application.Setup;
using Marsa.Infrastructure.Authentication;
using Marsa.Infrastructure.Docker;
using Marsa.Infrastructure.Persistence;
using Marsa.Infrastructure.Setup;
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
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddScoped<ISetupService, SetupService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
