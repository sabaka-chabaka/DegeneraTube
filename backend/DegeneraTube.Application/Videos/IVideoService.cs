using DegeneraTube.Shared;

namespace DegeneraTube.Application.Videos;

public interface IVideoService
{
    Task<Result<VideoDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<VideoPagedResponse>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<Result<VideoPagedResponse>> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<Result<VideoDto>> CreateAsync(Guid userId, VideoUploadRequest request, Stream videoStream, string fileName, CancellationToken ct = default);
    Task<Result<VideoDto>> UpdateAsync(Guid videoId, Guid requesterId, VideoUpdateRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid videoId, Guid requesterId, CancellationToken ct = default);
    Task<Result> RegisterViewAsync(Guid videoId, CancellationToken ct = default);
    Task<string?> GetStreamPathAsync(Guid videoId, CancellationToken ct = default);
    Task<string?> GetThumbnailPathAsync(Guid videoId, CancellationToken ct = default);
}