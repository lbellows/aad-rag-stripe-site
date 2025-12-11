using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AadRagStripeSite.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    protected int CurrentYear { get; } = DateTime.UtcNow.Year;

    protected bool IsAuthenticated { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (AuthenticationStateTask is not null)
        {
            var state = await AuthenticationStateTask;
            IsAuthenticated = state.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}
