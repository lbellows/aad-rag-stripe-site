using AadRagStripeSite.Services.Models;

namespace AadRagStripeSite.Services;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(UserContext context, CancellationToken cancellationToken);
    Task HandleWebhookAsync(HttpRequest request, CancellationToken cancellationToken);
}
