using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AadRagStripeSite.Infrastructure.Options;
using Azure.Core;
using Microsoft.Extensions.Options;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace AadRagStripeSite.Services.Foundry;

public sealed class FoundryAgentClient : IFoundryAgentClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private readonly HttpClient _httpClient;
    private readonly TokenCredential _credential;
    private readonly FoundryOptions _options;
    private readonly ILogger<FoundryAgentClient> _logger;

    public FoundryAgentClient(HttpClient httpClient, IOptions<FoundryOptions> options, ILogger<FoundryAgentClient> logger, TokenCredential? credential = null)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _credential = credential ?? new DefaultAzureCredential();
    }

    public async Task<FoundryAgentResponse> SendAsync(string conversationId, string userMessage, string? historyText, CancellationToken cancellationToken = default)
    {
        var token = await _credential.GetTokenAsync(new TokenRequestContext(new[] { _options.Scope }), cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.ResponsesEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        var payload = new FoundryAgentRequest
        {
            // Responses API accepts raw text; include history if provided.
            Input = historyText ?? userMessage,
            Stream = false,
            Metadata = new { conversationId },
            Model = string.IsNullOrWhiteSpace(_options.Model) ? null : _options.Model
        };

        var content = JsonSerializer.Serialize(payload, JsonOptions);
        request.Content = new StringContent(content, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Foundry request failed. Status: {Status} ({Reason}). Endpoint: {Endpoint}. Request: {RequestBody}. Response: {ResponseBody}",
                (int)response.StatusCode, response.ReasonPhrase, _options.ResponsesEndpoint, content, responseBody);
            response.EnsureSuccessStatusCode();
        }

        var doc = JsonDocument.Parse(responseBody);

        var outputText = ExtractOutputText(doc.RootElement);

        return new FoundryAgentResponse
        {
            OutputText = outputText,
            Raw = doc
        };
    }

    private static string? ExtractOutputText(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.String)
        {
            return root.GetString();
        }

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
            {
                var text = ExtractOutputText(item);
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }
        }

        // Try to locate "output_text" anywhere in the payload.
        if (TryFindProperty(root, "output_text", out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        // Try "output" array with first text element
        if (root.TryGetProperty("output", out var outputElem) && outputElem.ValueKind == JsonValueKind.Array && outputElem.GetArrayLength() > 0)
        {
            var first = outputElem[0];
            if (TryFindProperty(first, "content", out var contentElem) && contentElem.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in contentElem.EnumerateArray())
                {
                    if (TryFindProperty(item, "text", out var textElem) && textElem.ValueKind == JsonValueKind.String)
                    {
                        return textElem.GetString();
                    }
                }
            }
        }

        return null;
    }

    private static bool TryFindProperty(JsonElement element, string propertyName, out JsonElement found)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out found))
        {
            return true;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var child in element.EnumerateObject())
            {
                if (TryFindProperty(child.Value, propertyName, out found))
                {
                    return true;
                }
            }
        }

        found = default;
        return false;
    }
}
