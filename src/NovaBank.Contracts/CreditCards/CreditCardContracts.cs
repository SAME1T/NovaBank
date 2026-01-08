using NovaBank.Core.Enums;

namespace NovaBank.Contracts.CreditCards;

// --- Requests ---
public sealed record ApplyCreditCardRequest(
    decimal RequestedLimit,
    decimal MonthlyIncome
);

public sealed record ApproveCreditCardRequest(
    decimal ApprovedLimit
);

public sealed record RejectCreditCardRequest(
    string Reason
);

public sealed record CardPaymentRequest(
    decimal Amount,
    Guid FromAccountId
);

// --- Responses ---
public sealed record CreditCardApplicationResponse(
    Guid ApplicationId,
    Guid CustomerId,
    string CustomerName,
    decimal RequestedLimit,
    decimal? ApprovedLimit,
    decimal MonthlyIncome,
    string Status,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);

public sealed record CreditCardSummaryResponse(
    Guid CardId,
    string MaskedPan,
    decimal CreditLimit,
    decimal AvailableLimit,
    decimal CurrentDebt,
    DateTime? MinPaymentDueDate,
    decimal? MinPaymentAmount,
    string Status,
    int ExpiryMonth,
    int ExpiryYear
);

public sealed record MyApplicationResponse(
    Guid ApplicationId,
    decimal RequestedLimit,
    string Status,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);
