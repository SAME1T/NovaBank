using NovaBank.Core.Abstractions;

namespace NovaBank.Core.Entities;

public sealed class PasswordResetToken : Entity
{
    private PasswordResetToken() { }

    public Guid CustomerId { get; private set; }
    public string TargetEmail { get; private set; } = string.Empty;
    public string CodeHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public int AttemptCount { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public string? RequestedIp { get; private set; }
    public string? RequestedUserAgent { get; private set; }

    public PasswordResetToken(
        Guid customerId,
        string targetEmail,
        string codeHash,
        DateTime expiresAt,
        string? requestedIp = null,
        string? requestedUserAgent = null)
    {
        CustomerId = customerId;
        TargetEmail = targetEmail ?? throw new ArgumentNullException(nameof(targetEmail));
        CodeHash = codeHash ?? throw new ArgumentNullException(nameof(codeHash));
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        AttemptCount = 0;
        IsUsed = false;
        RequestedIp = requestedIp;
        RequestedUserAgent = requestedUserAgent;
    }

    public void IncrementAttempt()
    {
        AttemptCount++;
        TouchUpdated();
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        TouchUpdated();
    }
}

