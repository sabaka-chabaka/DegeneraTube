using DegeneraTube.Domain.Entities;
using DegeneraTube.Infrastructure.Persistence;
using DegeneraTube.Shared;
using Microsoft.EntityFrameworkCore;

namespace DegeneraTube.Infrastructure.Repositories;

public class CommentRepository(AppDbContext db) : BaseRepository<Comment>(db), ICommentRepository
{
    public async Task<PagedList<Comment>> GetByVideoIdAsync(
        Guid videoId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Set
            .AsNoTracking()
            .Where(c => c.VideoId == videoId && c.ParentId == null && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.User)
            .ToListAsync(ct);

        return PagedList<Comment>.Create(items, page, pageSize, total);
    }

    public async Task<List<Comment>> GetRepliesAsync(Guid parentId, CancellationToken ct = default) =>
        await Set
            .AsNoTracking()
            .Where(c => c.ParentId == parentId && !c.IsDeleted)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public async Task<Comment?> GetByIdWithUserAsync(Guid id, CancellationToken ct = default) =>
        await Set
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
}