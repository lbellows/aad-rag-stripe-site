using AadRagStripeSite.Services.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AadRagStripeSite.Components.Chat;

public sealed partial class RagChat : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private IJSObjectReference? _module;
    private DotNetObjectReference<RagChat>? _dotNetRef;
    private int? _streamId;
    private string userInput = string.Empty;
    private string responseText = string.Empty;
    private bool isStreaming;
    private string? statusMessage;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/chatClient.js");
            _dotNetRef = DotNetObjectReference.Create(this);
        }
    }

    private async Task SendAsync()
    {
        if (isStreaming || string.IsNullOrWhiteSpace(userInput))
        {
            return;
        }

        if (_module is null || _dotNetRef is null)
        {
            statusMessage = "Chat client not ready.";
            StateHasChanged();
            return;
        }

        responseText = string.Empty;
        statusMessage = null;
        isStreaming = true;

        var payload = new ChatRequest(userInput, ConversationId: null);
        _streamId = await _module.InvokeAsync<int>("streamChat", "/api/chat/stream", payload, _dotNetRef);
    }

    private async Task StopAsync()
    {
        if (!isStreaming || _module is null || !_streamId.HasValue)
        {
            return;
        }

        await _module.InvokeVoidAsync("cancelStream", _streamId.Value);
        isStreaming = false;
        statusMessage = "Stopped";
        StateHasChanged();
    }

    private Task HandleKeyDown(KeyboardEventArgs args)
    {
        if (args.Key is "Enter" && args.CtrlKey)
        {
            return SendAsync();
        }

        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnStreamChunk(string chunk)
    {
        responseText += (responseText.Length > 0 ? " " : string.Empty) + chunk;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnStreamCompleted()
    {
        isStreaming = false;
        statusMessage = "Completed";
        _streamId = null;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnStreamError(string error)
    {
        isStreaming = false;
        statusMessage = $"Error: {error}";
        _streamId = null;
        StateHasChanged();
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null && _streamId.HasValue)
        {
            try
            {
                await _module.InvokeVoidAsync("cancelStream", _streamId.Value);
            }
            catch
            {
                // Swallow disposal-time errors.
            }
        }

        _dotNetRef?.Dispose();
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
