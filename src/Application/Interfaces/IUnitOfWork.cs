namespace TaskManagement.Application.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }
    ITaskRepository Tasks { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    INotificationRepository Notifications { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
