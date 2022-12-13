using Microsoft.JSInterop;

namespace RiseDiary.Front.Shared;

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
