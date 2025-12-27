using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;

namespace NovaBank.Core.Entities;

/// <summary>
/// Onay akışı kaydı (Maker-Checker).
/// </summary>
public sealed class ApprovalWorkflow : Entity
{
    private ApprovalWorkflow() { }

    public ApprovalEntityType EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid RequestedById { get; private set; }
    public decimal? Amount { get; private set; }
    public Currency? Currency { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public UserRole RequiredRole { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public string? MetadataJson { get; private set; }

    public ApprovalWorkflow(
        ApprovalEntityType entityType,
        Guid entityId,
        Guid requestedById,
        UserRole requiredRole = UserRole.Manager,
        decimal? amount = null,
        Currency? currency = null,
        DateTime? expiresAt = null,
        string? metadataJson = null)
    {
        EntityType = entityType;
        EntityId = entityId;
        RequestedById = requestedById;
        RequiredRole = requiredRole;
        Amount = amount;
        Currency = currency;
        Status = ApprovalStatus.Pending;
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7); // Varsayılan 7 gün
        MetadataJson = metadataJson;
    }

    /// <summary>
    /// Onay ver.
    /// </summary>
    public void Approve(Guid approvedById)
    {
        if (Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("Sadece bekleyen onaylar onaylanabilir.");
        
        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
        {
            Status = ApprovalStatus.Expired;
            TouchUpdated();
            throw new InvalidOperationException("Onay süresi dolmuş.");
        }

        Status = ApprovalStatus.Approved;
        ApprovedById = approvedById;
        ApprovedAt = DateTime.UtcNow;
        TouchUpdated();
    }

    /// <summary>
    /// Reddet.
    /// </summary>
    public void Reject(Guid rejectedById, string reason)
    {
        if (Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("Sadece bekleyen onaylar reddedilebilir.");
        
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Red nedeni gerekli.", nameof(reason));

        Status = ApprovalStatus.Rejected;
        ApprovedById = rejectedById;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = reason.Trim();
        TouchUpdated();
    }

    /// <summary>
    /// Talep eden tarafından iptal et.
    /// </summary>
    public void Cancel()
    {
        if (Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("Sadece bekleyen onaylar iptal edilebilir.");

        Status = ApprovalStatus.Cancelled;
        TouchUpdated();
    }

    /// <summary>
    /// Süre doldu olarak işaretle.
    /// </summary>
    public void MarkExpired()
    {
        if (Status != ApprovalStatus.Pending)
            return;

        Status = ApprovalStatus.Expired;
        TouchUpdated();
    }

    /// <summary>
    /// Onayın süresi dolmuş mu kontrol eder.
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}
