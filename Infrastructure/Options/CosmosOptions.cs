using System.ComponentModel.DataAnnotations;

namespace AadRagStripeSite.Infrastructure.Options;

public sealed class CosmosOptions
{
    [Required]
    [Url]
    public required string AccountEndpoint { get; init; }

    [Required]
    public required string Database { get; init; }

    [Required]
    public required string Container { get; init; }

    /// <summary>
    /// Optional key for Cosmos access. Omit when using managed identity + RBAC.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    /// Optional connection string. Use this or Key/managed identity.
    /// </summary>
    public string? ConnectionString { get; init; }
}
