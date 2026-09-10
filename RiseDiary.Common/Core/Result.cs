using System.Text.Json.Serialization;

namespace RiseDiary.Common.Core;

public class Result
{
    public string? Message { get; }
    public bool Succeeded { get; }

    private static Result _empryResult = new(true, null);
    public static Result EmpryResult => _empryResult;

    [JsonConstructor]
    protected Result(bool succeeded, string? message)
    {
        Succeeded = succeeded;
        Message = message;
    }

    public static Result Success(string? message = null)
    {
        return new Result(true, message);
    }

    public static Result Failure(string message)
    {
        return new Result(false, message);
    }

    public static implicit operator Result(string? message) => string.IsNullOrEmpty(message)
        ? Success()
        : Failure(message);
}


public sealed class Result<T> : Result
{
    public T? Data { get; }

    [JsonConstructor]
    private Result(bool succeeded, string? message, T? data) : base(succeeded, message)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T>(true, message, data);
    }

    public new static Result<T> Failure(string message)
    {
        return new Result<T>(false, message, default);
    }
}