namespace NovaBank.Core.Enums;

/// <summary>
/// Hesap durumları.
/// </summary>
public enum AccountStatus
{
    Active = 0,
    Frozen = 1,
    Closed = 2,
    PendingApproval = 3  // Döviz hesapları için admin onayı bekliyor
}

