using System.Runtime.CompilerServices;
using AadRagStripeSite.Services.Data;
using AadRagStripeSite.Services.Models;
using Microsoft.Extensions.Options;
using AadRagStripeSite.Infrastructure.Options;

namespace AadRagStripeSite.Services;

/// <summary>
/// Placeholder RAG implementation that stores chat messages in Cosmos and emits canned streaming data.
/// Replace the response generation with Azure Search + OpenAI calls.
/// </summary>
public sealed class RagChatService : IRagChatService
{
    private static readonly string[] DemoChunks =
    [
        "This is a placeholder response for the RAG chatbot.",
        "Wire Azure AI Search + Azure OpenAI to deliver grounded answers.",
        "Streaming is enabled via Server-Sent Events."
    ];

    private readonly IChatRepository _chatRepository;
    private readonly IOptions<CosmosOptions> _cosmosOptions;

    public RagChatService(IChatRepository chatRepository, IOptions<CosmosOptions> cosmosOptions)
    {
        _chatRepository = chatRepository;
        _cosmosOptions = cosmosOptions;
    }

    public Task<string> GetAnswerAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        var text = string.Join(" ", DemoChunks);
        return Task.FromResult(text);
    }

    public async IAsyncEnumerable<string> StreamAnswerAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var chunk in DemoChunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return chunk;
            await Task.Delay(150, cancellationToken);
        }
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
}
