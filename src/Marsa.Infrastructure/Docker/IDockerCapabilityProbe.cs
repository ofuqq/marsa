namespace Marsa.Infrastructure.Docker;

public interface IDockerCapabilityProbe
{
    DockerCapabilities GetCapabilities();
}

public sealed record DockerCapabilities(
    string Version,
    bool BuildKit,
    string Architecture,
    bool CgroupAvailable)
{
    public bool IsUsable => !string.Equals(Version, "unavailable", StringComparison.OrdinalIgnoreCase);
}
