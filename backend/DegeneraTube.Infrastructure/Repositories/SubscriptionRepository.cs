using DegeneraTube.Domain.Entities;
using DegeneraTube.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DegeneraTube.Infrastructure.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> GetAsync(Guid subscriberId, Guid channelId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid subscriberId, Guid channelId, CancellationToken ct = default);
    Task<int> CountSubscribersAsync(Guid channelId, CancellationToken ct = default);
    Task<List<Subscription>> GetBySubscriberAsync(Guid subscriberId, CancellationToken ct = default);
}

public class SubscriptionRepository(AppDbContext db)
    : BaseRepository<Subscription>(db), ISubscriptionRepository
{
    public async Task<Subscription?> GetAsync(
        Guid subscriberId, Guid channelId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(
            s => s.SubscriberId == subscriberId && s.ChannelId == channelId, ct);

    public async Task<bool> ExistsAsync(
        Guid subscriberId, Guid channelId, CancellationToken ct = default) =>
        await Set.AnyAsync(
            s => s.SubscriberId == subscriberId && s.ChannelId == channelId, ct);

    public async Task<int> CountSubscribersAsync(Guid channelId, CancellationToken ct = default) =>
        await Set.CountAsync(s => s.ChannelId == channelId, ct);

    public async Task<List<Subscription>> GetBySubscriberAsync(
        Guid subscriberId, CancellationToken ct = default) =>
        await Set
            .AsNoTracking()
            .Where(s => s.SubscriberId == subscriberId)
            .Include(s => s.Channel)
            .ToListAsync(ct);
}