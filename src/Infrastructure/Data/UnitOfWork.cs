using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;

namespace TaskManagement.Infrastructure.Data;

/// <summary>Coordinates repositories and commits changes as a single transaction.</summary>
public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private IUserRepository? _users;
    private ITaskRepository? _tasks;
    private IRefreshTokenRepository? _refreshTokens;
    private INotificationRepository? _notifications;

    public IUserRepository Users => _users ??= new UserRepository(context);
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(context);
    public INotificationRepository Notifications => _notifications ??= new NotificationRepository(context);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);

    public async ValueTask DisposeAsync() => await context.DisposeAsync();
}
