using DegeneraTube.Domain.Entities;
using DegeneraTube.Domain.Enums;
using DegeneraTube.Infrastructure.Persistence;
using DegeneraTube.Shared;
using Microsoft.EntityFrameworkCore;

namespace DegeneraTube.Infrastructure.Repositories;

public class VideoRepository(AppDbContext db) : BaseRepository<Video>(db), IVideoRepository
{
    public async Task<Video?> GetByIdWithUserAsync(Guid id, CancellationToken ct = default) =>
        await Set
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<PagedList<Video>> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var query = Set
            .AsNoTracking()
            .Where(v => v.Status == VideoStatus.Ready)
            .Where(v => search == null || v.Title.Contains(search) || v.Tags.Contains(search))
            .OrderByDescending(v => v.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(v => v.User)
            .ToListAsync(ct);

        return PagedList<Video>.Create(items, page, pageSize, total);
    }

    public async Task<PagedList<Video>> GetByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Set
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedList<Video>.Create(items, page, pageSize, total);
    }

    public async Task<List<Video>> GetByStatusAsync(VideoStatus status, CancellationToken ct = default) =>
        await Set
            .Where(v => v.Status == status)
            .ToListAsync(ct);

    public async Task IncrementViewCountAsync(Guid id, CancellationToken ct = default) =>
        await Set
            .Where(v => v.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.ViewCount, v => v.ViewCount + 1), ct);
}