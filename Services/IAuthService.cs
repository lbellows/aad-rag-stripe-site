using System.Security.Claims;
using AadRagStripeSite.Services.Models;

namespace AadRagStripeSite.Services;

public interface IAuthService
{
    Task<UserContext> GetUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}
