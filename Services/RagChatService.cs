using System.Runtime.CompilerServices;
using AadRagStripeSite.Infrastructure.Options;
using AadRagStripeSite.Services.Data;
using AadRagStripeSite.Services.Foundry;
using AadRagStripeSite.Services.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace AadRagStripeSite.Services;

public sealed class RagChatService : IRagChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IFoundryAgentClient _foundryClient;
    private readonly IOptions<CosmosOptions> _cosmosOptions;

    public RagChatService(IChatRepository chatRepository, IFoundryAgentClient foundryClient, IOptions<CosmosOptions> cosmosOptions)
    {
        _chatRepository = chatRepository;
        _foundryClient = foundryClient;
        _cosmosOptions = cosmosOptions;
    }

    public async Task<string> GetAnswerAsync(ChatRequest request, string userId, CancellationToken cancellationToken)
    {
        var history = await BuildHistoryAsync(userId, request.ConversationId ?? "default", cancellationToken);
        await PersistUserMessageAsync(userId, request, cancellationToken);
        var response = await _foundryClient.SendAsync(request.ConversationId ?? "default", request.Message, history, cancellationToken);
        var assistantText = response.OutputText ?? "(no response)";
        await PersistAssistantMessageAsync(userId, request.ConversationId ?? "default", assistantText, cancellationToken);
        return assistantText;
    }

    public async IAsyncEnumerable<string> StreamAnswerAsync(ChatRequest request, string userId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var answer = await GetAnswerAsync(request, userId, cancellationToken);
        yield return answer;
    }

    public async Task PersistUserMessageAsync(string userId, ChatRequest request, CancellationToken cancellationToken)
    {
        var message = new ChatMessage(
            Id: Guid.NewGuid().ToString("N"),
            UserId: userId,
            ConversationId: request.ConversationId ?? "default",
            Role: "user",
            Content: request.Message,
            CreatedAtUtc: DateTimeOffset.UtcNow);

        await _chatRepository.SaveAsync(message, cancellationToken);
    }

    private async Task PersistAssistantMessageAsync(string userId, string conversationId, string text, CancellationToken cancellationToken)
    {
        var message = new ChatMessage(
            Id: Guid.NewGuid().ToString("N"),
            UserId: userId,
            ConversationId: conversationId,
            Role: "assistant",
            Content: text,
            CreatedAtUtc: DateTimeOffset.UtcNow);

        await _chatRepository.SaveAsync(message, cancellationToken);
    }

    private async Task<string> BuildHistoryAsync(string userId, string conversationId, CancellationToken cancellationToken)
    {
        var history = await _chatRepository.GetConversationAsync(userId, conversationId, cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Conversation so far:");
        foreach (var turn in history)
        {
            sb.Append(turn.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? "Assistant: " : "User: ");
            sb.AppendLine(turn.Content);
        }
        return sb.ToString();
    }
}
