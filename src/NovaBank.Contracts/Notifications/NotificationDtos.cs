using NovaBank.Core.Enums;

namespace NovaBank.Contracts.Notifications;

public record NotificationResponse(
    Guid Id,
    NotificationType Type,
    string Title,
    string Message,
    NotificationStatus Status,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? SentAt,
    DateTime? ReadAt
);

public record CreateNotificationRequest(
    Guid CustomerId,
    NotificationType Type,
    string Title,
    string Message,
    string? MetadataJson = null
);

public record NotificationPreferenceResponse(
    Guid Id,
    NotificationType NotificationType,
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled
);

public record UpdateNotificationPreferenceRequest(
    NotificationType NotificationType,
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled
);
