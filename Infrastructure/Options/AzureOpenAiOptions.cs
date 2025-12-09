using System.ComponentModel.DataAnnotations;

namespace AadRagStripeSite.Infrastructure.Options;

public sealed class AzureOpenAiOptions
{
    [Required]
    [Url]
    public required string Endpoint { get; init; }

    [Required]
    public required string DeploymentName { get; init; }

    [Required]
    public required string ApiVersion { get; init; }

    /// <summary>
    /// Optional API key if not using managed identity.
    /// </summary>
    public string? ApiKey { get; init; }
}
