using Microsoft.AspNetCore.Components;

namespace AadRagStripeSite.Pages;

public partial class PilotChat : ComponentBase
{
    [Inject] private Services.ChatSessionService ChatSession { get; set; } = default!;

    private IReadOnlyList<Services.ChatMessageTurn> Messages => ChatSession.History;
    private string userInput = string.Empty;
    private bool isBusy;
    private string? error;

    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            return;
        }

        isBusy = true;
        error = null;
        try
        {
            var text = userInput;
            userInput = string.Empty;
            await ChatSession.SendUserMessageAsync(text);
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
        finally
        {
            isBusy = false;
            StateHasChanged();
        }
    }
}
