using Microsoft.AspNetCore.Components;

namespace AadRagStripeSite.Pages;

public partial class Home : ComponentBase
{
    public sealed record FeatureCard(string Title, string Description, string Tag);

    private static readonly IReadOnlyList<FeatureCard> _features =
    [
        new("Azure-native RAG", "Azure OpenAI + Azure AI Search + Blob Storage, orchestrated with minimal APIs and SSE streaming.", "Grounded answers"),
        new("Auth-ready shell", "Routes scoped under /app with a dark UI that expects Entra ID or B2C for registration and sign-in.", "Identity-first"),
        new("Stripe billing", "Stripe Checkout, customer/subscription IDs, and webhook-driven entitlement updates for paid tiers.", "Monetize"),
        new("Modern Blazor", "net10.0 + C# 14, persistent component state, and the built-in reconnection modal for Server interactivity.", "Platform")
    ];

    protected IReadOnlyList<FeatureCard> Features => _features;
}
