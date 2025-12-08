using System.Security.Claims;
using AadRagStripeSite.Services.Models;

namespace AadRagStripeSite.Services.Stub;

/// <summary>
/// Placeholder auth service that extracts basic identity data from the ClaimsPrincipal.
/// Replace with Entra ID / B2C integration.
/// </summary>
public sealed class StubAuthService : IAuthService
{
    public Task<UserContext> GetUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        if (principal.Identity?.IsAuthenticated is true)
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.Identity.Name;
            var email = principal.FindFirstValue(ClaimTypes.Email);
            return Task.FromResult(new UserContext(userId, email, true));
        }

        return Task.FromResult(UserContext.Anonymous);
    }
}
