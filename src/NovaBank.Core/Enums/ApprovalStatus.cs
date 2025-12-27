namespace NovaBank.Core.Enums;

/// <summary>
/// Onay durumları.
/// </summary>
public enum ApprovalStatus
{
    /// <summary>Onay bekliyor</summary>
    Pending = 0,
    /// <summary>Onaylandı</summary>
    Approved = 1,
    /// <summary>Reddedildi</summary>
    Rejected = 2,
    /// <summary>İptal edildi</summary>
    Cancelled = 3,
    /// <summary>Süresi doldu</summary>
    Expired = 4
}
