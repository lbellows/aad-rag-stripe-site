using AadRagStripeSite.Services.Models;

namespace AadRagStripeSite.Services;

public interface IRagChatService
{
    Task<string> GetAnswerAsync(ChatRequest request, CancellationToken cancellationToken);
    IAsyncEnumerable<string> StreamAnswerAsync(ChatRequest request, CancellationToken cancellationToken);
}
