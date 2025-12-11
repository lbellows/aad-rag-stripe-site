using System.ComponentModel.DataAnnotations;

namespace AadRagStripeSite.Infrastructure.Options;

public sealed class FoundryOptions
{
    public string? ResponsesEndpoint { get; init; }
    public string? Scope { get; init; }
    public string? Model { get; init; }

    // Azure.AI.Projects path
    public string? ProjectEndpoint { get; init; }
    public string? AgentName { get; init; }
    public bool UseProjects { get; init; }
}
