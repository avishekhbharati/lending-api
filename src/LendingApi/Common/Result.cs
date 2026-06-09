namespace LendingApi.Common;

public enum ResultError
{
    None,
    NotFound,
    Conflict,
    Validation
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ResultError Error { get; }
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, T? value, ResultError error, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) =>
        new(true, value, ResultError.None, null);

    public static Result<T> NotFound(string? message = null) =>
        new(false, default, ResultError.NotFound, message);

    public static Result<T> Conflict(string message) =>
        new(false, default, ResultError.Conflict, message);

    public static Result<T> Validation(string message) =>
        new(false, default, ResultError.Validation, message);
}