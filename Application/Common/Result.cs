namespace Application.Common;

public enum ErrorType
{
    None = 0,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    Validation = 422
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    protected Result(bool isSuccess, string? error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null, ErrorType.None);

    public static Result Failure(string error, ErrorType errorType = ErrorType.BadRequest) =>
        new(false, error, errorType);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? error, ErrorType errorType)
        : base(isSuccess, error, errorType)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null, ErrorType.None);

    public new static Result<T> Failure(string error, ErrorType errorType = ErrorType.BadRequest) =>
        new(false, default, error, errorType);
}
