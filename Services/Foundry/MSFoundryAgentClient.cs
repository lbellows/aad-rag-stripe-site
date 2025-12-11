using System.Linq;
using System.Reflection;
using System.Text.Json;
using AadRagStripeSite.Infrastructure.Options;
using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AadRagStripeSite.Services.Foundry;

public sealed class MSFoundryAgentClient : IFoundryAgentClient
{
    private readonly ILogger<MSFoundryAgentClient> _logger;
    private readonly AIProjectClient _projectClient;
    private readonly string _agentName;
    private AgentRecord? _cachedAgent;

    public MSFoundryAgentClient(IOptions<FoundryOptions> options, ILogger<MSFoundryAgentClient> logger)
    {
        _logger = logger;
        var opts = options.Value;

        if (string.IsNullOrWhiteSpace(opts.ProjectEndpoint))
        {
            throw new InvalidOperationException("Foundry ProjectEndpoint is required when UseProjects is enabled.");
        }

        if (string.IsNullOrWhiteSpace(opts.AgentName))
        {
            throw new InvalidOperationException("Foundry AgentName is required when UseProjects is enabled.");
        }

        _agentName = opts.AgentName;
        _projectClient = new AIProjectClient(new Uri(opts.ProjectEndpoint), new DefaultAzureCredential());
    }

    public async Task<FoundryAgentResponse> SendAsync(string conversationId, string userMessage, string? historyText, CancellationToken cancellationToken = default)
    {
        var agent = await GetAgentAsync(cancellationToken);
        var responseClient = _projectClient.OpenAI.GetProjectResponsesClientForAgent(agent);
        var responseResult = await responseClient.CreateResponseAsync(historyText ?? userMessage, cancellationToken: cancellationToken);
        var response = responseResult.Value;
        var outputText = response.GetOutputText();

        JsonDocument raw;
        try
        {
            raw = JsonDocument.Parse(JsonSerializer.Serialize(response));
        }
        catch
        {
            raw = JsonDocument.Parse("{}");
        }

        return new FoundryAgentResponse
        {
            OutputText = outputText,
            Raw = raw
        };
    }

    private async Task<AgentRecord> GetAgentAsync(CancellationToken cancellationToken)
    {
        if (_cachedAgent is not null)
        {
            return _cachedAgent;
        }

        var agentResult = await _projectClient.Agents.GetAgentAsync(_agentName, cancellationToken: cancellationToken);
        _cachedAgent = agentResult.Value ?? throw new InvalidOperationException($"Agent '{_agentName}' not found.");
        _logger.LogInformation("Using Foundry agent {AgentName} (id: {AgentId})", _cachedAgent.Name, _cachedAgent.Id);
        return _cachedAgent;
    }
}
