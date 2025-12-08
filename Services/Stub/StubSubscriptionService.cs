using AadRagStripeSite.Services.Models;

namespace AadRagStripeSite.Services.Stub;

/// <summary>
/// Placeholder subscription service that treats all users as active free-tier with ample quota.
/// Replace with a database-backed implementation wired to Stripe webhooks.
/// </summary>
public sealed class StubSubscriptionService : ISubscriptionService
{
    private const int DefaultQuota = 100;

    public Task<SubscriptionInfo> GetSubscriptionAsync(UserContext context, CancellationToken cancellationToken)
    {
        var info = new SubscriptionInfo(SubscriptionTier.Free, IsActive: true, CurrentPeriodEnd: null, RemainingMessages: DefaultQuota);
        return Task.FromResult(info);
    }

    public Task<bool> TryConsumeMessageAsync(UserContext context, CancellationToken cancellationToken)
    {
        // For now always allow; real implementation should decrement quota atomically.
        return Task.FromResult(true);
    }
}
