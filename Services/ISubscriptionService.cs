using AadRagStripeSite.Services.Models;

namespace AadRagStripeSite.Services;

public interface ISubscriptionService
{
    Task<SubscriptionInfo> GetSubscriptionAsync(UserContext context, CancellationToken cancellationToken);
    Task<bool> TryConsumeMessageAsync(UserContext context, CancellationToken cancellationToken);
}
