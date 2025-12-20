namespace NovaBank.Application.Common.Results;

public class Result
{
    public bool IsSuccess { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    protected Result(bool isSuccess, string? errorCode = null, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true);
    public static Result Failure(string errorCode, string errorMessage) => new(false, errorCode, errorMessage);
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    private Result(bool isSuccess, T? value, string? errorCode = null, string? errorMessage = null)
        : base(isSuccess, errorCode, errorMessage)
    {
        Value = value;
    }

    public static new Result<T> Success(T value) => new(true, value);
    public static new Result<T> Failure(string errorCode, string errorMessage) => new(false, default, errorCode, errorMessage);
}

