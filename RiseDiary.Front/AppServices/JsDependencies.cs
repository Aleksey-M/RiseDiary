using Microsoft.JSInterop;

namespace RiseDiary.Front.AppServices;

public sealed class JsDependencies : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _calendarLibTask;
    private readonly Lazy<Task<IJSObjectReference>> _initJcropModuleTask;

    public JsDependencies(IJSRuntime jsRuntime)
    {
        _calendarLibTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./lib/calendar/dist/js-year-calendar.min.js").AsTask());

        _initJcropModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/initJcropLib.js").AsTask());
    }

    public async Task InitYearCalendar()
    {
        _ = await _calendarLibTask.Value;
    }

    public async Task InitJcrop()
    {
        var initLibModule = await _initJcropModuleTask.Value;
        await initLibModule.InvokeVoidAsync("initJcropLib");
    }

    public async ValueTask DisposeAsync()
    {
        if (_calendarLibTask.IsValueCreated)
        {
            var module = await _calendarLibTask.Value;
            await module.DisposeAsync();
        }
    }
}
