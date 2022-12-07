using Microsoft.JSInterop;

namespace RiseDiary.Front.JsComponents;

/// <summary>
/// Ошибка, возникшая при ожидании завершения js-кода
/// </summary>
public class JsException : Exception
{
    public JsException(string message) : base(message)
    {
    }
}

/// <summary>
/// Класс для ожидания завершения асинхронного js-кода
/// </summary>
public class CallbacksTask
{
    private TaskCompletionSource _tcs = new();

    public DotNetObjectReference<CallbacksTask> CreateRefForJs() => DotNetObjectReference.Create(this);

    public Task WaitTask => _tcs.Task;

    [JSInvokable]
    public Task Completed()
    {
        _tcs.SetResult();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task Error(string message)
    {
        _tcs.SetException(new JsException(message));
        return Task.CompletedTask;
    }
}

/// <summary>
/// Класс для ожидания завершения асинхронного и возвращающего значение js-кода
/// </summary>
/// <typeparam name="T">Тип возвращающегося из js значения</typeparam>
public class CallbacksTask<T>
{
    private TaskCompletionSource<T> _tcs = new();

    public DotNetObjectReference<CallbacksTask<T>> CreateRefForJs() => DotNetObjectReference.Create(this);

    public Task<T> WaitTask => _tcs.Task;

    [JSInvokable]
    public Task Completed(T resultFronJs)
    {
        _tcs.SetResult(resultFronJs);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task Error(string message)
    {
        _tcs.SetException(new JsException(message));
        return Task.CompletedTask;
    }
}
