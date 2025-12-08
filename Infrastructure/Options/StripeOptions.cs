using System.ComponentModel.DataAnnotations;

namespace AadRagStripeSite.Infrastructure.Options;

public sealed class StripeOptions
{
    [Required]
    public required string PublishableKey { get; init; }

    [Required]
    public required string SecretKey { get; init; }

    [Required]
    public required string WebhookSecret { get; init; }
}
