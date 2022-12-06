using Microsoft.JSInterop;

namespace RiseDiary.Front.JsHelpers;

public sealed class DateClickHandler
{
    private readonly Action<DateOnly> Handler;

    public DateClickHandler(Action<DateOnly> handler)
    {
        Handler = handler;
    }

    [JSInvokable]
    public Task OnClick(DateOnly date)
    {
        Handler.Invoke(date);
        return Task.CompletedTask;
    }

    public DotNetObjectReference<DateClickHandler> CreateRefForJs() => DotNetObjectReference.Create(this);
}

public sealed class CalendarComponent : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;
    private readonly Lazy<Task<IJSObjectReference>> calendarLibTask;

    public CalendarComponent(IJSRuntime jsRuntime)
    {
        calendarLibTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./lib/calendar/dist/js-year-calendar.min.js").AsTask());

        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/calendar-component.js").AsTask());
    }

    private async Task<IJSObjectReference> LoadModule()
    {
        _ = await calendarLibTask.Value;
        return await moduleTask.Value;
    }

    public async Task<DotNetObjectReference<DateClickHandler>> CreateCalendar(List<DateOnly> dates, Action<DateOnly> onDateClick)
    {
        var module = await LoadModule();

        var dateClickHandler = new DateClickHandler(onDateClick);
        var dateClickHandlerRef = dateClickHandler.CreateRefForJs();

        var callbacks = new CallbacksTask();
        using var callbacksJsRef = callbacks.CreateRefForJs();

        await module.InvokeVoidAsync("initCalendar", "#calendar", dates, dateClickHandlerRef, callbacksJsRef);
        await callbacks.WaitTask;

        return dateClickHandlerRef;
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }

        if (calendarLibTask.IsValueCreated)
        {
            var module = await calendarLibTask.Value;
            await module.DisposeAsync();
        }
    }
}
