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
}
