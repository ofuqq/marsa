using Marsa.Infrastructure.Docker;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Marsa.Server.Health;

public sealed class DockerHealthCheck(IDockerCapabilityProbe probe) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var capabilities = probe.GetCapabilities();
        var data = new Dictionary<string, object>
        {
            ["version"] = capabilities.Version,
            ["buildKit"] = capabilities.BuildKit,
            ["architecture"] = capabilities.Architecture,
            ["cgroupAvailable"] = capabilities.CgroupAvailable,
        };

        if (!capabilities.IsUsable)
        {
            return Task.FromResult(HealthCheckResult.Degraded("Docker daemon is not available for deployments.", data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Docker is available.", data));
    }
}
