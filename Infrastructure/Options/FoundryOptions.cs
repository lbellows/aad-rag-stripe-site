using System.ComponentModel.DataAnnotations;

namespace AadRagStripeSite.Infrastructure.Options;

public sealed class FoundryOptions
{
    [Required]
    [Url]
    public required string ResponsesEndpoint { get; init; }

    [Required]
    [Url]
    public required string Scope { get; init; }

    /// <summary>
    /// Agent/model identifier required by the Responses endpoint (e.g., agent name or ID).
    /// </summary>
    public string? Model { get; init; }
}
