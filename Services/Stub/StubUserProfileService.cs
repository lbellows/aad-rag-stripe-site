using AadRagStripeSite.Services.Models;

namespace AadRagStripeSite.Services.Stub;

/// <summary>
/// Placeholder user profile service that echoes the identity until a database-backed profile exists.
/// </summary>
public sealed class StubUserProfileService : IUserProfileService
{
    public Task<UserProfile> GetOrCreateAsync(UserContext context, CancellationToken cancellationToken)
    {
        var id = context.UserId ?? "anon";
        var profile = new UserProfile(id, context.Email, context.Email ?? "Anonymous");
        return Task.FromResult(profile);
    }
}
