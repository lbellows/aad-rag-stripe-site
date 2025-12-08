using System.ComponentModel.DataAnnotations;

namespace AadRagStripeSite.Infrastructure.Options;

public sealed class UsageLimitOptions
{
    [Range(1, int.MaxValue)]
    public int FreeMessagesPerDay { get; init; } = 25;

    [Range(1, int.MaxValue)]
    public int ProMessagesPerDay { get; init; } = 250;
}
