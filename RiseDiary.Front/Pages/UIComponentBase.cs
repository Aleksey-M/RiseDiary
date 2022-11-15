using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RiseDiary.Shared;

namespace RiseDiary.Front.Pages;

public class UIComponentBase : ComponentBase, IDisposable
{
    private readonly Lazy<CancellationTokenSource> _cts = new(() => new CancellationTokenSource(), isThreadSafe: false);

    protected CancellationToken Token => _cts.Value.Token;

    protected bool Loading { get; private set; }

    protected string? ErrorMessage { get; set; }

    protected bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    protected async Task StartApiRequest()
    {
        Loading = true;
        StateHasChanged();
        await Task.Yield();
    }

    protected async Task FinishApiRequest(string? errorMessage)
    {
        Loading = false;
        ErrorMessage = errorMessage;
        StateHasChanged();
        await Task.Yield();
    }

    protected async Task<T?> GetCheckedResponseContent<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }

        var msg = await response.Content.ReadFromJsonAsync<MessageResponse>();

        ErrorMessage = $"{response.StatusCode}: {msg?.Message ?? string.Empty}";
        Loading = false;
        StateHasChanged();
        await Task.Yield();

        return default;
    }

    protected async Task<bool> CheckSuccessResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return true;

        var msg = await response.Content.ReadFromJsonAsync<MessageResponse>();

        ErrorMessage = $"{response.StatusCode}: {msg?.Message ?? string.Empty}";
        Loading = false;
        StateHasChanged();
        await Task.Yield();

        return false;
    }

    public void Dispose()
    {
        if (_cts.IsValueCreated)
        {
            _cts.Value.Cancel();
        }
    }
}
