using AadRagStripeSite.Services;
using AadRagStripeSite.Services.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AadRagStripeSite.Components.Subscriptions;

public sealed partial class SubscriptionSummary : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private ISubscriptionService SubscriptionService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private SubscriptionInfo? info;

    protected override async Task OnInitializedAsync()
    {
        var user = await AuthService.GetUserAsync((await AuthenticationStateProvider.GetAuthenticationStateAsync()).User, CancellationToken.None);
        info = await SubscriptionService.GetSubscriptionAsync(user, CancellationToken.None);
    }
}
