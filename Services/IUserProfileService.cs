using AadRagStripeSite.Services.Models;

namespace AadRagStripeSite.Services;

public interface IUserProfileService
{
    Task<UserProfile> GetOrCreateAsync(UserContext context, CancellationToken cancellationToken);
}
