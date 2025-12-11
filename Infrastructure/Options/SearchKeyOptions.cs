using System.ComponentModel.DataAnnotations;

namespace AadRagStripeSite.Infrastructure.Options;

public sealed class SearchKeyOptions
{

    /// <summary>
    /// Query key for client-side or limited search scenarios.
    /// </summary>
    [Required]
    public required string QueryKey { get; init; }
}
