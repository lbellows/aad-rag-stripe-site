namespace AadRagStripeSite.Services.Foundry;

public interface IFoundryAgentClient
{
    Task<FoundryAgentResponse> SendAsync(string conversationId, string userMessage, string? historyText, CancellationToken cancellationToken = default);
}
