using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.Repositories;

public sealed class RefreshTokenRepository(ApplicationDbContext context)
    : Repository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken ct = default) =>
        await DbSet
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow, ct);

    public async Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        await DbSet
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.IsRevoked, true)
                .SetProperty(r => r.RevokedReason, reason), ct);
    }
}
