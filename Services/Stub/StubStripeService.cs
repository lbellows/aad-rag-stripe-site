using AadRagStripeSite.Services.Models;

namespace AadRagStripeSite.Services.Stub;

/// <summary>
/// Placeholder Stripe service. Replace with stripe-dotnet integration and webhook verification.
/// </summary>
public sealed class StubStripeService : IStripeService
{
    public Task<string> CreateCheckoutSessionAsync(UserContext context, CancellationToken cancellationToken)
    {
        var url = "https://billing.stripe.com/p/test-placeholder";
        return Task.FromResult(url);
    }

    public Task HandleWebhookAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        // Acknowledge immediately; real implementation should verify signature and persist subscription updates.
        return Task.CompletedTask;
    }
}
