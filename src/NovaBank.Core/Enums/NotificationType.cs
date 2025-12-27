namespace NovaBank.Core.Enums;

/// <summary>
/// Bildirim türleri.
/// </summary>
public enum NotificationType
{
    /// <summary>SMS</summary>
    Sms = 0,
    /// <summary>E-posta</summary>
    Email = 1,
    /// <summary>Push bildirimi</summary>
    Push = 2,
    /// <summary>Uygulama içi bildirim</summary>
    InApp = 3
}
