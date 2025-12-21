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
    public const string HesapDondurulmus = "AccountFrozen";
    public const string HesapKapali = "AccountClosed";
    public const string KapatmakIcinBakiyeSifirOlmali = "BalanceMustBeZeroToClose";
    public const string KapaliHesapTekrarAcilamaz = "ClosedAccountCannotBeReactivated";
    public const string ResetTokenNotFoundOrExpired = "ResetTokenNotFoundOrExpired";
    public const string InvalidResetCode = "InvalidResetCode";
    public const string EmailSendFailed = "EmailSendFailed";
    public const string AlreadyReversed = "AlreadyReversed";
    public const string CannotReverseReversal = "CannotReverseReversal";
    public const string ReversalWindowExpired = "ReversalWindowExpired";
    public const string ExternalReversalNotSupported = "ExternalReversalNotSupported";
}

