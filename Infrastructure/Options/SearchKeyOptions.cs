using System.ComponentModel.DataAnnotations;

namespace AadRagStripeSite.Infrastructure.Options;

public sealed class SearchKeyOptions
{
    /// <summary>
    /// Admin key for index management and queries. Required unless using Managed Identity.
    /// </summary>
    [Required]
    public required string AdminKey { get; init; }

    /// <summary>
    /// Query key for client-side or limited search scenarios.
    /// </summary>
    [Required]
    public required string QueryKey { get; init; }
}
