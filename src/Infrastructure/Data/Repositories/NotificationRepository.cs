using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.Repositories;

public sealed class NotificationRepository(ApplicationDbContext context)
    : Repository<Notification>(context), INotificationRepository
{
    public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default) =>
        await DbSet.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default) =>
        await DbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
}
