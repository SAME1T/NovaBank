using Mapster;
using NovaBank.Application.Common;
using NovaBank.Application.Common.Errors;
using NovaBank.Application.Common.Interfaces;
using NovaBank.Contracts.Notifications;
using NovaBank.Core.Entities;
using NovaBank.Core.Enums;
using NovaBank.Core.Exceptions;

namespace NovaBank.Application.Notifications;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CurrentUser _currentUser;

    public NotificationService(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        CurrentUser currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> SendNotificationAsync(CreateNotificationRequest request, CancellationToken ct = default)
    {
        // Actually, this should be internal only or admin only
        if (!_currentUser.IsAdmin) 
            throw new UnauthorizedAccessException("Only admin can send notifications manually.");

        var notification = new Notification(
            request.CustomerId,
            request.Type,
            request.Title,
            request.Message,
            request.MetadataJson
        );

        // Here we would integrate with SMS/Email provider
        // Mock sending
        notification.MarkSent();

        await _repository.AddAsync(notification, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return notification.Id;
    }

    public async Task<List<NotificationResponse>> GetMyNotificationsAsync(int take = 50, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.CustomerId.HasValue)
             throw new UnauthorizedAccessException();

        var list = await _repository.GetByCustomerIdAsync(_currentUser.CustomerId.Value, take, ct);
        return list.Adapt<List<NotificationResponse>>();
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.CustomerId.HasValue)
             throw new UnauthorizedAccessException();
        
        var list = await _repository.GetUnreadByCustomerIdAsync(_currentUser.CustomerId.Value, ct);
        return list.Count;
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.CustomerId.HasValue)
             throw new UnauthorizedAccessException();

        var notification = await _repository.GetByIdAsync(id, ct);
        if (notification == null) throw new NotFoundException("Notification not found.");

        if (notification.CustomerId != _currentUser.CustomerId.Value)
            throw new UnauthorizedAccessException("Cannot access other user's notification.");

        notification.MarkRead();
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.CustomerId.HasValue)
             throw new UnauthorizedAccessException();

        var unread = await _repository.GetUnreadByCustomerIdAsync(_currentUser.CustomerId.Value, ct);
        foreach (var n in unread)
        {
            n.MarkRead();
        }
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
