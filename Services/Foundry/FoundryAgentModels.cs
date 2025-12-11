using System.Text.Json;

namespace AadRagStripeSite.Services.Foundry;

public sealed class FoundryAgentRequest
{
    public object? Input { get; set; }
    public string? ConversationId { get; set; }
    public bool? Stream { get; set; }
    public object? Metadata { get; set; }
}

public sealed class FoundryAgentResponse
{
    public string? OutputText { get; set; }
    public JsonDocument Raw { get; init; } = default!;
}
