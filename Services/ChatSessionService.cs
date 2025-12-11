using System.Text;
using AadRagStripeSite.Services.Foundry;

namespace AadRagStripeSite.Services;

public sealed class ChatMessageTurn
{
    public required string Sender { get; init; } // "user" or "assistant"
    public required string Text { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public sealed class ChatSessionService
{
    private readonly IFoundryAgentClient _agentClient;
    private readonly List<ChatMessageTurn> _history = new();
    private readonly string _conversationId = Guid.NewGuid().ToString("N");

    public ChatSessionService(IFoundryAgentClient agentClient)
    {
        _agentClient = agentClient;
    }

    public IReadOnlyList<ChatMessageTurn> History => _history;
    public string ConversationId => _conversationId;

    public async Task SendUserMessageAsync(string text, CancellationToken cancellationToken = default)
    {
        var userTurn = new ChatMessageTurn
        {
            Sender = "user",
            Text = text,
            Timestamp = DateTimeOffset.UtcNow
        };
        _history.Add(userTurn);

        var historyText = ComposeHistory(text);
        var response = await _agentClient.SendAsync(_conversationId, text, historyText, cancellationToken);
        var assistantText = response.OutputText ?? "(no response)";

        var assistantTurn = new ChatMessageTurn
        {
            Sender = "assistant",
            Text = assistantText,
            Timestamp = DateTimeOffset.UtcNow
        };
        _history.Add(assistantTurn);
    }

    private string ComposeHistory(string latestUserText)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Conversation so far:");
        foreach (var turn in _history)
        {
            sb.Append(turn.Sender.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? "Assistant: " : "User: ");
            sb.AppendLine(turn.Text);
        }
        sb.AppendLine();
        sb.AppendLine("New question:");
        sb.AppendLine(latestUserText);
        return sb.ToString();
    }
}
