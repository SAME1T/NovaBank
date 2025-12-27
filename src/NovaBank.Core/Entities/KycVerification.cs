using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;

namespace NovaBank.Core.Entities;

/// <summary>
/// KYC doğrulama kaydı.
/// </summary>
public sealed class KycVerification : Entity
{
    private KycVerification() { }

    public Guid CustomerId { get; private set; }
    public VerificationType VerificationType { get; private set; }
    public VerificationStatus Status { get; private set; }
    public string? DocumentPath { get; private set; }
    public Guid? VerifiedById { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public string? MetadataJson { get; private set; }

    public KycVerification(
        Guid customerId,
        VerificationType verificationType,
        string? documentPath = null,
        DateTime? expiresAt = null,
        string? metadataJson = null)
    {
        CustomerId = customerId;
        VerificationType = verificationType;
        Status = VerificationStatus.Pending;
        DocumentPath = documentPath?.Trim();
        ExpiresAt = expiresAt;
        MetadataJson = metadataJson;
    }

    /// <summary>
    /// Doğrula.
    /// </summary>
    public void Verify(Guid verifiedById, DateTime? expiresAt = null)
    {
        if (Status != VerificationStatus.Pending)
            throw new InvalidOperationException("Sadece bekleyen doğrulamalar onaylanabilir.");

        Status = VerificationStatus.Verified;
        VerifiedById = verifiedById;
        VerifiedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddYears(1); // Varsayılan 1 yıl geçerli
        TouchUpdated();
    }

    /// <summary>
    /// Reddet.
    /// </summary>
    public void Reject(Guid rejectedById, string reason)
    {
        if (Status != VerificationStatus.Pending)
            throw new InvalidOperationException("Sadece bekleyen doğrulamalar reddedilebilir.");
        
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Red nedeni gerekli.", nameof(reason));

        Status = VerificationStatus.Rejected;
        VerifiedById = rejectedById;
        VerifiedAt = DateTime.UtcNow;
        RejectionReason = reason.Trim();
        TouchUpdated();
    }

    /// <summary>
    /// Süre doldu olarak işaretle.
    /// </summary>
    public void MarkExpired()
    {
        if (Status != VerificationStatus.Verified)
            return;

        Status = VerificationStatus.Expired;
        TouchUpdated();
    }

    /// <summary>
    /// Belge yolunu güncelle.
    /// </summary>
    public void UpdateDocument(string documentPath)
    {
        DocumentPath = documentPath?.Trim();
        TouchUpdated();
    }

    /// <summary>
    /// Doğrulama geçerli mi kontrol eder.
    /// </summary>
    public bool IsValid => Status == VerificationStatus.Verified && 
                           (!ExpiresAt.HasValue || DateTime.UtcNow <= ExpiresAt.Value);
}
