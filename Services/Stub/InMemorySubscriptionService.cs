using System.Collections.Concurrent;
using AadRagStripeSite.Infrastructure.Options;
using AadRagStripeSite.Services.Models;
using Microsoft.Extensions.Options;

namespace AadRagStripeSite.Services.Stub;

/// <summary>
/// In-memory subscription/quota tracker for development. Treats all users as free tier with a daily quota.
/// </summary>
public sealed class InMemorySubscriptionService : ISubscriptionService
{
    private readonly UsageLimitOptions _limits;
    private readonly ConcurrentDictionary<string, QuotaState> _store = new();

    public InMemorySubscriptionService(IOptions<UsageLimitOptions> options)
    {
        _limits = options.Value;
    }

    public Task<SubscriptionInfo> GetSubscriptionAsync(UserContext context, CancellationToken cancellationToken)
    {
        var (record, _) = GetOrCreate(context);
        var info = new SubscriptionInfo(SubscriptionTier.Free, IsActive: true, CurrentPeriodEnd: record.ResetsAt, RemainingMessages: record.Remaining);
        return Task.FromResult(info);
    }

    public Task<bool> TryConsumeMessageAsync(UserContext context, CancellationToken cancellationToken)
    {
        var (record, key) = GetOrCreate(context);
        if (record.Remaining <= 0)
        {
            return Task.FromResult(false);
        }

        var updated = record with { Remaining = record.Remaining - 1 };
        _store[key] = updated;
        return Task.FromResult(true);
    }

    private (QuotaState record, string key) GetOrCreate(UserContext context)
    {
        var key = context.UserId ?? "anon";
        var now = DateTimeOffset.UtcNow;
        return (_store.AddOrUpdate(key,
            _ => new QuotaState(_limits.FreeMessagesPerDay, now.AddDays(1)),
            (_, existing) =>
            {
                if (existing.ResetsAt <= now)
                {
                    return new QuotaState(_limits.FreeMessagesPerDay, now.AddDays(1));
                }

                return existing;
            }), key);
    }

    private sealed record QuotaState(int Remaining, DateTimeOffset ResetsAt);
}
