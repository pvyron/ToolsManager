namespace ToolsManager.Abstractions.Models;

public sealed class Result
{
    private Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }
    
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public sealed class Result<T>
{
    private Result(bool isSuccess, Error<T> error, T? value)
    {
        if (isSuccess && error != Error<T>.None ||
            !isSuccess && error == Error<T>.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        if (isSuccess && value is null ||
            !isSuccess && value is not null)
        {
            throw new ArgumentException("Invalid value", nameof(value));
        }

        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }
    
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error<T> Error { get; }
    public T? Value { get; }

    public static Result<T> Success(T value) => new(true, Error<T>.None, value);
    public static Result<T> Failure(Error<T> error) => new(false, error, default);

    public static implicit operator Result<T>(T value) => new Result<T>(true, Error<T>.None, value);
}