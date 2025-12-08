using System.ComponentModel.DataAnnotations;

namespace AadRagStripeSite.Infrastructure.Options;

public sealed class AzureSearchOptions
{
    [Required]
    [Url]
    public required string Endpoint { get; init; }

    [Required]
    public required string IndexName { get; init; }
}
