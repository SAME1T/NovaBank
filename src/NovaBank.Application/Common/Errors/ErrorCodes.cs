namespace NovaBank.Application.Common.Errors;

public static class ErrorCodes
{
    public const string NotFound = "NOT_FOUND";
    public const string Validation = "VALIDATION_ERROR";
    public const string Conflict = "CONFLICT";
    public const string InsufficientFunds = "INSUFFICIENT_FUNDS";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string InvalidOperation = "INVALID_OPERATION";
    public const string CurrencyMismatch = "CURRENCY_MISMATCH";
    public const string InvalidAmount = "INVALID_AMOUNT";
    public const string AccountNotFound = "ACCOUNT_NOT_FOUND";
    public const string CustomerNotFound = "CUSTOMER_NOT_FOUND";
    public const string SameAccountTransfer = "SAME_ACCOUNT_TRANSFER";
}

