namespace AadRagStripeSite.Services.Models;

public enum SubscriptionTier
{
    Free = 0,
    Pro = 1
}

public sealed record SubscriptionInfo(
    SubscriptionTier Tier,
    bool IsActive,
    DateTimeOffset? CurrentPeriodEnd,
    int RemainingMessages);
