using AadRagStripeSite.Services.Models;
using Microsoft.Azure.Cosmos;

namespace AadRagStripeSite.Services.Data;

public interface IChatRepository
{
    Task<ChatMessage> SaveAsync(ChatMessage message, CancellationToken cancellationToken);
    Task<IReadOnlyList<ChatMessage>> GetConversationAsync(string userId, string conversationId, CancellationToken cancellationToken);
}

public sealed class CosmosChatRepository : IChatRepository
{
    private readonly Container _container;

    public CosmosChatRepository(Container container)
    {
        _container = container;
    }

    public Task<ChatMessage> SaveAsync(ChatMessage message, CancellationToken cancellationToken)
        => UpsertAsync(message, cancellationToken);

    public async Task<IReadOnlyList<ChatMessage>> GetConversationAsync(string userId, string conversationId, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId AND c.conversationId = @conversationId ORDER BY c.createdAtUtc")
            .WithParameter("@userId", userId)
            .WithParameter("@conversationId", conversationId);

        var results = new List<ChatMessage>();
        using var feed = _container.GetItemQueryIterator<ChatMessage>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) });
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }
        return results;
    }

    private async Task<ChatMessage> UpsertAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        var response = await _container.UpsertItemAsync(message, new PartitionKey(message.UserId), cancellationToken: cancellationToken);
        return response.Resource;
    }
}
