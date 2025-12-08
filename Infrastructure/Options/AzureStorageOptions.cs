using System.ComponentModel.DataAnnotations;

namespace AadRagStripeSite.Infrastructure.Options;

public sealed class AzureStorageOptions
{
    [Required]
    public required string AccountName { get; init; }
}
