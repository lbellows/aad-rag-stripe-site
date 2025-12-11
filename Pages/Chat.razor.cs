using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Markdig;

namespace AadRagStripeSite.Pages;

[Authorize]
public partial class Chat : ComponentBase
{
    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    [Inject] private Services.ChatSessionService ChatSession { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private IReadOnlyList<Services.ChatMessageTurn> Messages => ChatSession.History;
    private string userInput = string.Empty;
    private bool isBusy;
    private string? error;
    private ElementReference chatWindowRef;
    private IJSObjectReference? _module;
    private TimeSpan? _clientOffset;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/chatClient.js");
            var offsetMinutes = await _module.InvokeAsync<int>("getTimezoneOffset");
            _clientOffset = TimeSpan.FromMinutes(-offsetMinutes);
        }
    }

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
            var sendTask = ChatSession.SendUserMessageAsync(text);

            // Render + scroll after the user turn is added, then again after the assistant responds.
            await InvokeAsync(StateHasChanged);
            await ScrollToBottomAsync();

            await sendTask;
            await InvokeAsync(StateHasChanged);
            await ScrollToBottomAsync();
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
        finally
        {
            isBusy = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private static string RenderMarkdown(string text) =>
        Markdown.ToHtml(text ?? string.Empty, MarkdownPipeline);

    private string FormatTimestamp(DateTimeOffset timestamp)
    {
        if (_clientOffset is null)
        {
            return timestamp.ToLocalTime().ToString("t");
        }

        return timestamp.ToOffset(_clientOffset.Value).ToString("t");
    }

    private async Task ScrollToBottomAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("scrollToBottom", chatWindowRef);
        }
        catch (JSDisconnectedException)
        {
            // Circuit ended before scroll could run.
        }
    }
}
