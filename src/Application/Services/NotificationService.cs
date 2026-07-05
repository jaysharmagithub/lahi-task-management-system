using AutoMapper;
using TaskManagement.Application.DTOs.Notification;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Application.Services;

public sealed class NotificationService(IUnitOfWork uow, IMapper mapper) : INotificationService
{
    public async Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default)
    {
        var notifications = await uow.Notifications.GetUserNotificationsAsync(userId, ct);
        return mapper.Map<IReadOnlyList<NotificationDto>>(notifications);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
    {
        var notification = await uow.Notifications.GetByIdAsync(notificationId, ct)
            ?? throw new NotFoundException("Notification", notificationId);

        if (notification.UserId != userId)
            throw new ForbiddenException("Access denied.");

        notification.IsRead = true;
        uow.Notifications.Update(notification);
        await uow.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        await uow.Notifications.MarkAllReadAsync(userId, ct);
        await uow.SaveChangesAsync(ct);
    }
}
