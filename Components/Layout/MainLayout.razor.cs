using Microsoft.AspNetCore.Components;

namespace AadRagStripeSite.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    protected int CurrentYear { get; } = DateTime.UtcNow.Year;
}
