using NovaBank.Core.Abstractions;
using NovaBank.Core.Enums;

namespace NovaBank.Core.Entities;

/// <summary>
/// Bildirim kaydı.
/// </summary>
public sealed class Notification : Entity
{
    private Notification() { }

    public Guid CustomerId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? MetadataJson { get; private set; }

    public Notification(
        Guid customerId,
        NotificationType notificationType,
        string title,
        string message,
        string? metadataJson = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Bildirim başlığı gerekli.", nameof(title));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Bildirim mesajı gerekli.", nameof(message));

        CustomerId = customerId;
        NotificationType = notificationType;
        Title = title.Trim();
        Message = message.Trim();
        Status = NotificationStatus.Pending;
        MetadataJson = metadataJson;
    }

    /// <summary>
    /// Gönderildi olarak işaretle.
    /// </summary>
    public void MarkSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        TouchUpdated();
    }

    /// <summary>
    /// Başarısız olarak işaretle.
    /// </summary>
    public void MarkFailed()
    {
        Status = NotificationStatus.Failed;
        TouchUpdated();
    }

    /// <summary>
    /// Okundu olarak işaretle.
    /// </summary>
    public void MarkRead()
    {
        if (Status == NotificationStatus.Sent || Status == NotificationStatus.Pending)
        {
            Status = NotificationStatus.Read;
            ReadAt = DateTime.UtcNow;
            TouchUpdated();
        }
    }
}
