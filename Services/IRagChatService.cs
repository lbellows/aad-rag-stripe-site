using AadRagStripeSite.Services.Models;

namespace AadRagStripeSite.Services;

public interface IRagChatService
{
    Task<string> GetAnswerAsync(ChatRequest request, CancellationToken cancellationToken);
    IAsyncEnumerable<string> StreamAnswerAsync(ChatRequest request, CancellationToken cancellationToken);
    Task PersistUserMessageAsync(string userId, ChatRequest request, CancellationToken cancellationToken);
}
