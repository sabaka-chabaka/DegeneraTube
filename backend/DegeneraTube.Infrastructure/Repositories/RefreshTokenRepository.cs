using DegeneraTube.Domain.Entities;
using DegeneraTube.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DegeneraTube.Infrastructure.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}

public class RefreshTokenRepository(AppDbContext db)
    : BaseRepository<RefreshToken>(db), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        await Set
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token, ct);

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true), ct);
}