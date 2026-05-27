using DegeneraTube.Domain.Entities;
using DegeneraTube.Domain.Enums;
using DegeneraTube.Shared;

namespace DegeneraTube.Infrastructure.Repositories;

public interface IVideoRepository : IRepository<Video>
{
    Task<Video?> GetByIdWithUserAsync(Guid id, CancellationToken ct = default);
    Task<PagedList<Video>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<PagedList<Video>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<List<Video>> GetByStatusAsync(VideoStatus status, CancellationToken ct = default);
    Task IncrementViewCountAsync(Guid id, CancellationToken ct = default);
    Task<int> CountByUserAsync(Guid userId, CancellationToken ct = default);
}