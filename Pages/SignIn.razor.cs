using Microsoft.AspNetCore.Components;

namespace AadRagStripeSite.Pages;

public partial class SignIn : ComponentBase
{
    [Inject] private NavigationManager Nav { get; set; } = default!;

    protected override void OnInitialized()
    {
        Nav.NavigateTo("/auth/signin", forceLoad: true);
    }
}
