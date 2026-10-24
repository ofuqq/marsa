using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Marsa.Infrastructure.Docker;

/// <summary>
/// Reports whether a Docker daemon and BuildKit are available without owning a
/// container. Docker is optional during startup.
/// </summary>
public sealed class DockerCapabilityProbe : IDockerCapabilityProbe
{
    public DockerCapabilities GetCapabilities()
    {
        var version = ReadVersion();
        var buildKit = version != "unavailable" && ReadBuildKit();
        var architecture = RuntimeInformation.ProcessArchitecture.ToString();
        var cgroup = Directory.Exists("/sys/fs/cgroup");

        return new DockerCapabilities(version, buildKit, architecture, cgroup);
    }

    private static string ReadVersion()
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo("docker", "version --format {{.Server.Version}}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            process.Start();
            if (!process.WaitForExit(3000))
            {
                process.Kill();
                return "unavailable";
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output)
                ? output
                : "unavailable";
        }
        catch
        {
            return "unavailable";
        }
    }

    private static bool ReadBuildKit()
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo("docker", "buildx version")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            process.Start();
            if (!process.WaitForExit(3000))
            {
                process.Kill();
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
