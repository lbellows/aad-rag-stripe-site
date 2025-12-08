namespace AadRagStripeSite.Services.Models;

public sealed record UserContext(string? UserId, string? Email, bool IsAuthenticated)
{
    public static UserContext Anonymous { get; } = new(null, null, false);
}
