using Microsoft.AspNetCore.Components;

namespace AadRagStripeSite.Pages;

public partial class Home : ComponentBase
{
    public sealed record FeatureCard(string Title, string Description, string Tag);

    private static readonly IReadOnlyList<FeatureCard> _features =
    [
        new("Airline-grade RAG", "Grounded responses cite FCOM, QRH, and ops manuals with Azure AI Search and Azure OpenAI.", "Flight ops"),
        new("Identity for crews", "Protected /app routes ready for Entra ID or B2C so pilots and dispatch sign in quickly.", "Auth-ready"),
        new("Subscription control", "Stripe Checkout + webhook updates drive per-user quotas for pro crews and training tiers.", "Monetize"),
        new("Modern Blazor shell", "net10.0 + C# 14 with persistent state and reconnection support for reliable cockpit tooling.", "Platform")
    ];

    protected IReadOnlyList<FeatureCard> Features => _features;
}
