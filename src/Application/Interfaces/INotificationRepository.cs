using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
}
