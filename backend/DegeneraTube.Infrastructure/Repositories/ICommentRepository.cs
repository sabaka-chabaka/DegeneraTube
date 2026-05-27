using DegeneraTube.Domain.Entities;
using DegeneraTube.Shared;

namespace DegeneraTube.Infrastructure.Repositories;

public interface ICommentRepository : IRepository<Comment>
{
    Task<PagedList<Comment>> GetByVideoIdAsync(Guid videoId, int page, int pageSize, CancellationToken ct = default);
    Task<List<Comment>> GetRepliesAsync(Guid parentId, CancellationToken ct = default);
    Task<Comment?> GetByIdWithUserAsync(Guid id, CancellationToken ct = default);
}