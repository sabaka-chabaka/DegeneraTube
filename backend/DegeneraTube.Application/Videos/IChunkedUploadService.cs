using DegeneraTube.Shared;

namespace DegeneraTube.Application.Videos;

public interface IChunkedUploadService
{
    Task<Result<InitUploadResponse>> InitAsync(
        Guid userId,
        InitUploadRequest request,
        CancellationToken ct = default);

    Task<Result<UploadChunkResponse>> SaveChunkAsync(
        Guid uploadId,
        Guid userId,
        int chunkIndex,
        int totalChunks,
        Stream chunkStream,
        CancellationToken ct = default);

    Task<Result<VideoDto>> FinalizeAsync(
        Guid uploadId,
        Guid userId,
        CancellationToken ct = default);
}