namespace AadRagStripeSite.Services.Models;

public sealed record ChatMessage(
    string Id,
    string UserId,
    string ConversationId,
    string Role,
    string Content,
    DateTimeOffset CreatedAtUtc);
