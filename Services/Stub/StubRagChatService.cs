using System.Runtime.CompilerServices;
using AadRagStripeSite.Services.Models;

namespace AadRagStripeSite.Services.Stub;

/// <summary>
/// Placeholder RAG chat service that returns a canned response.
/// Replace with Azure AI Search + Azure OpenAI integration and streaming.
/// </summary>
public sealed class StubRagChatService : IRagChatService
{
    private static readonly string[] DemoChunks =
    [
        "This is a placeholder response for the RAG chatbot.",
        "Wire Azure AI Search + Azure OpenAI to deliver grounded answers.",
        "Streaming is enabled via Server-Sent Events."
    ];

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
}
