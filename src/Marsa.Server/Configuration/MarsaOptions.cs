using System.ComponentModel.DataAnnotations;

namespace Marsa.Server.Configuration;

/// <summary>
/// Strongly typed configuration for the parts of Marsa that must be known
/// before the server starts serving traffic.
/// </summary>
public sealed class MarsaOptions
{
    [Required(ErrorMessage = "Set Marsa:Database:ConnectionString to a PostgreSQL connection string.")]
    public string DatabaseConnectionString { get; set; } = string.Empty;

    [Required(ErrorMessage = "Set Marsa:Security:MasterKeyPath to a file containing the master key.")]
    public string MasterKeyPath { get; set; } = string.Empty;

    [Required(ErrorMessage = "Set Marsa:Edge:CaddyAdminSocket to the Caddy admin Unix socket.")]
    public string CaddyAdminSocket { get; set; } = string.Empty;

    [Required(ErrorMessage = "Set Marsa:PublicBaseUrl to the public URL users will open.")]
    [Url(ErrorMessage = "Marsa:PublicBaseUrl must be an absolute HTTP or HTTPS URL.")]
    public string PublicBaseUrl { get; set; } = string.Empty;

    public string[] TrustedProxies { get; set; } = Array.Empty<string>();
}
