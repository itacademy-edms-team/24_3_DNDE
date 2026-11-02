namespace Common.Application;

/// <summary>
/// Machine-readable error code and human-readable message for a failed operation.
/// </summary>
public sealed record Error(string Code, string Message);

/// <summary>
/// Result of an operation: either a value or an error, no exceptions for expected failures.
/// </summary>
public class Result<T>
{
    private readonly T? _value;

    private Result(T value) { _value = value; IsSuccess = true; }

    private Result(Error error) { Error = error; }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Result has no value (error: {Error?.Code}).");

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);
}
