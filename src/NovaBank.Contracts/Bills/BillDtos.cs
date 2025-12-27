using NovaBank.Core.Enums;

namespace NovaBank.Contracts.Bills;

public record BillInstitutionResponse(
    Guid Id,
    string Code,
    string Name,
    BillCategory Category,
    string? LogoUrl,
    bool IsActive
);

public record CreateBillInstitutionRequest(
    string Code,
    string Name,
    BillCategory Category,
    string? LogoUrl = null
);

public record UpdateBillInstitutionRequest(
    string Name,
    BillCategory Category,
    string? LogoUrl = null,
    bool? IsActive = null
);

public record BillInquiryRequest(
    Guid InstitutionId,
    string SubscriberNo
);

public record BillInquiryResponse(
    Guid InstitutionId,
    string InstitutionName,
    string SubscriberNo,
    string? SubscriberName,
    decimal Amount,
    decimal Commission,
    DateTime? DueDate,
    string? InvoiceNo
);

public record PayBillRequest(
    Guid? AccountId,
    Guid? CardId,
    Guid InstitutionId,
    string SubscriberNo,
    decimal Amount, // Verification to ensure inquiry match
    string? InvoiceNo = null
);

public record BillPaymentResponse(
    Guid Id,
    Guid? AccountId,
    Guid? CardId,
    Guid InstitutionId,
    string InstitutionName,
    string SubscriberNo,
    decimal Amount,
    decimal Commission,
    decimal TotalAmount,
    DateTime PaidAt,
    string ReferenceCode,
    PaymentStatus Status
);
