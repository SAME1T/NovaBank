using NovaBank.Core.Entities;

namespace NovaBank.Application.Common.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Notification>> GetByCustomerIdAsync(Guid customerId, int take = 50, CancellationToken ct = default);
    Task<List<Notification>> GetUnreadByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
}
