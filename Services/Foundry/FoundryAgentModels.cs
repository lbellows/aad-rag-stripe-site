using System.Text.Json;
using System.Text.Json.Serialization;

namespace AadRagStripeSite.Services.Foundry;

public sealed class FoundryAgentRequest
{
    [JsonPropertyName("input")]
    public object? Input { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }
}

public sealed class FoundryAgentResponse
{
    public string? OutputText { get; set; }
    public JsonDocument Raw { get; init; } = default!;
}
