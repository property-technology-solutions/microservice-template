namespace BuildingBlocks.Application;

/// <summary>
/// Represents the result of an operation without a return value
/// Provides a functional approach to error handling
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected init; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; protected init; } = string.Empty;
    public List<string> Errors { get; protected init; } = new();

    protected Result(bool isSuccess, string message = "")
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public static Result Success(string message = "") => new(true, message);
    public static Result Fail(string error) => new(false) { Errors = new List<string> { error } };
    public static Result Fail(List<string> errors) => new(false) { Errors = errors };
}

/// <summary>
/// Represents the result of an operation with a return value
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; private init; }

    private Result(bool isSuccess, T? value = default, string message = "") : base(isSuccess, message)
    {
        Value = value;
    }

    public static Result<T> Success(T value, string message = "") => new(true, value, message);
    public new static Result<T> Fail(string error) => new(false) { Errors = new List<string> { error } };
    public new static Result<T> Fail(List<string> errors) => new(false) { Errors = errors };
}

