using Microsoft.EntityFrameworkCore;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;

namespace NovaBank.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly BankDbContext _context;

    public NotificationRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Notifications.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<Notification>> GetByCustomerIdAsync(Guid customerId, int take = 50, CancellationToken ct = default)
    {
        return await _context.Notifications
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<List<Notification>> GetUnreadByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Notifications
            .Where(x => x.CustomerId == customerId && x.ReadAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        await _context.Notifications.AddAsync(notification, ct);
    }
}
