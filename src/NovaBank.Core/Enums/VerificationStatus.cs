namespace NovaBank.Core.Enums;

/// <summary>
/// Doğrulama durumları.
/// </summary>
public enum VerificationStatus
{
    /// <summary>Beklemede</summary>
    Pending = 0,
    /// <summary>Doğrulandı</summary>
    Verified = 1,
    /// <summary>Reddedildi</summary>
    Rejected = 2,
    /// <summary>Süresi doldu</summary>
    Expired = 3
}
