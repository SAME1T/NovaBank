using NovaBank.Contracts.Notifications;

namespace NovaBank.Application.Notifications;

public interface INotificationService
{
    Task<Guid> SendNotificationAsync(CreateNotificationRequest request, CancellationToken ct = default);
    Task<List<NotificationResponse>> GetMyNotificationsAsync(int take = 50, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(CancellationToken ct = default);
    Task MarkAsReadAsync(Guid id, CancellationToken ct = default);
    Task MarkAllAsReadAsync(CancellationToken ct = default);
}
