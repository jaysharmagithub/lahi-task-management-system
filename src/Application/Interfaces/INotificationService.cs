using TaskManagement.Application.DTOs.Notification;

namespace TaskManagement.Application.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}
