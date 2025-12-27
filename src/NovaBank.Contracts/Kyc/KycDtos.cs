using NovaBank.Core.Enums;

namespace NovaBank.Contracts.Kyc;

public record CreateKycVerificationRequest(
    VerificationType VerificationType,
    string DocumentPath,
    DateTime? ExpiryDate = null
);

public record UpdateKycVerificationRequest(
    string? DocumentPath,
    DateTime? ExpiryDate,
    bool? IsVerified = null
);

public record KycVerificationResponse(
    Guid Id,
    Guid CustomerId,
    VerificationType VerificationType,
    VerificationStatus Status,
    string DocumentPath,
    DateTime? VerifiedAt,
    Guid? VerifiedById,
    string? RejectionReason,
    DateTime? ExpiryDate,
    bool IsActive
);
