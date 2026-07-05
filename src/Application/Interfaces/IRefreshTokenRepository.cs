using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken ct = default);
}
