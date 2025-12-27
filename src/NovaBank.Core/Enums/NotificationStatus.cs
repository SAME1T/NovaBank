namespace NovaBank.Core.Enums;

/// <summary>
/// Bildirim durumları.
/// </summary>
public enum NotificationStatus
{
    /// <summary>Beklemede</summary>
    Pending = 0,
    /// <summary>Gönderildi</summary>
    Sent = 1,
    /// <summary>Başarısız</summary>
    Failed = 2,
    /// <summary>Okundu</summary>
    Read = 3
}
