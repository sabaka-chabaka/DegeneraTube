using DegeneraTube.Domain.Entities;
using DegeneraTube.Domain.Enums;
using DegeneraTube.Infrastructure.Processing;
using DegeneraTube.Infrastructure.Repositories;
using DegeneraTube.Infrastructure.Storage;
using DegeneraTube.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DegeneraTube.Application.Videos;

public class ChunkedUploadService(
    IServiceScopeFactory scopeFactory,
    IFileStorage storage,
    ILogger<ChunkedUploadService> logger) : IChunkedUploadService
{
    public async Task<Result<InitUploadResponse>> InitAsync(
        Guid userId,
        InitUploadRequest request,
        CancellationToken ct = default)
    {
        var video = new Video
        {
            UserId      = userId,
            Title       = request.Title,
            Description = request.Description,
            Tags        = request.Tags ?? [],
            Status      = VideoStatus.Processing
        };

        var meta = System.Text.Json.JsonSerializer.Serialize(new UploadMeta(
            request.FileName,
            request.FileSize,
            request.TotalChunks));

        video.Description = $"__upload_meta__{meta}__{request.Description}";

        using var scope = scopeFactory.CreateScope();
        var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
        await videos.AddAsync(video, ct);
        await videos.SaveAsync(ct);

        var tempDir = TempDir(video.Id);
        Directory.CreateDirectory(storage.GetFullPath(tempDir));

        logger.LogInformation("Upload session {Id} initiated ({Chunks} chunks)", video.Id, request.TotalChunks);

        return Result.Success(new InitUploadResponse(video.Id));
    }

    public async Task<Result<UploadChunkResponse>> SaveChunkAsync(
        Guid uploadId,
        Guid userId,
        int chunkIndex,
        int totalChunks,
        Stream chunkStream,
        CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
        var video  = await videos.GetByIdAsync(uploadId, ct);

        if (video is null)
            return Result.Failure<UploadChunkResponse>("Upload session not found.");

        if (!video.IsOwnedBy(userId))
            return Result.Failure<UploadChunkResponse>("Access denied.");

        var tempDir      = TempDir(uploadId);
        var chunkRelPath = Path.Combine(tempDir, $"{chunkIndex}.chunk");

        await storage.SaveAsync(chunkStream, tempDir, $"{chunkIndex}.chunk", ct);

        var fullTempDir       = storage.GetFullPath(tempDir);
        var receivedChunks    = Directory.GetFiles(fullTempDir, "*.chunk").Length;
        var isComplete        = receivedChunks >= totalChunks;

        logger.LogDebug("Upload {Id}: chunk {Idx}/{Total} saved", uploadId, chunkIndex, totalChunks);

        return Result.Success(new UploadChunkResponse(receivedChunks, totalChunks, isComplete));
    }

    public async Task<Result<VideoDto>> FinalizeAsync(
        Guid uploadId,
        Guid userId,
        CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
        var video  = await videos.GetByIdWithUserAsync(uploadId, ct);

        if (video is null)
            return Result.Failure<VideoDto>("Upload session not found.");

        if (!video.IsOwnedBy(userId))
            return Result.Failure<VideoDto>("Access denied.");

        var (fileName, originalDescription) = ExtractMeta(video.Description);
        video.Description = originalDescription;
        videos.Update(video);
        await videos.SaveAsync(ct);

        _ = AssembleAndProcessAsync(uploadId, fileName);

        return Result.Success(ToDto(video));
    }
    
    private async Task AssembleAndProcessAsync(Guid uploadId, string fileName)
    {
        try
        {
            var tempDir     = TempDir(uploadId);
            var fullTempDir = storage.GetFullPath(tempDir);

            var chunkFiles = Directory
                .GetFiles(fullTempDir, "*.chunk")
                .OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f)))
                .ToArray();

            var rawFolder  = Path.Combine("raw", uploadId.ToString());
            var rawDir     = storage.GetFullPath(rawFolder);
            Directory.CreateDirectory(rawDir);
            var rawFullPath = Path.Combine(rawDir, fileName);

            await using (var output = File.Create(rawFullPath))
            {
                foreach (var chunk in chunkFiles)
                {
                    await using var input = File.OpenRead(chunk);
                    await input.CopyToAsync(output);
                }
            }

            logger.LogInformation("Upload {Id}: chunks assembled into {File}", uploadId, rawFullPath);

            Directory.Delete(fullTempDir, recursive: true);

            using var scope      = scopeFactory.CreateScope();
            var ffmpeg           = scope.ServiceProvider.GetRequiredService<FfmpegService>();
            var thumbnails       = scope.ServiceProvider.GetRequiredService<ThumbnailService>();
            var videosRepo       = scope.ServiceProvider.GetRequiredService<IVideoRepository>();

            var hlsOutputDir = storage.GetFullPath(Path.Combine("hls", uploadId.ToString()));
            var hlsResult    = await ffmpeg.TranscodeToHlsAsync(rawFullPath, hlsOutputDir);

            var thumbDir  = storage.GetFullPath(Path.Combine("thumbnails", uploadId.ToString()));
            var probe     = await ffmpeg.ProbeAsync(rawFullPath);
            await thumbnails.ExtractBestAsync(rawFullPath, thumbDir, probe.DurationSeconds);

            var video = await videosRepo.GetByIdAsync(uploadId);
            if (video is null) return;

            video.Status          = VideoStatus.Ready;
            video.HlsPath         = Path.Combine("hls", uploadId.ToString()).Replace('\\', '/');
            video.ThumbnailPath   = Path.Combine("thumbnails", uploadId.ToString(), "thumb.jpg").Replace('\\', '/');
            video.DurationSeconds = probe.DurationSeconds;
            video.Resolutions     = hlsResult.Resolutions;

            videosRepo.Update(video);
            await videosRepo.SaveAsync();

            File.Delete(rawFullPath);

            logger.LogInformation("Upload {Id}: processing complete", uploadId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Upload {Id}: processing failed", uploadId);

            using var scope      = scopeFactory.CreateScope();
            var videosRepo       = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
            var video            = await videosRepo.GetByIdAsync(uploadId);
            if (video is null) return;
            video.Status = VideoStatus.Failed;
            videosRepo.Update(video);
            await videosRepo.SaveAsync();
        }
    }
    
    private static string TempDir(Guid uploadId) =>
        Path.Combine("temp", uploadId.ToString());
    
    private static (string fileName, string? description) ExtractMeta(string? raw)
    {
        if (raw is null || !raw.StartsWith("__upload_meta__"))
            return ("video.mp4", raw);

        var after   = raw["__upload_meta__".Length..];
        var jsonEnd = after.IndexOf("}__");
        if (jsonEnd < 0) return ("video.mp4", raw);

        var json        = after[..(jsonEnd + 1)];
        var description = after[(jsonEnd + 3)..];
        if (string.IsNullOrWhiteSpace(description)) description = null;

        try
        {
            var meta = System.Text.Json.JsonSerializer.Deserialize<UploadMeta>(json);
            return (meta?.FileName ?? "video.mp4", description);
        }
        catch
        {
            return ("video.mp4", description);
        }
    }

    private static VideoDto ToDto(Video v) =>
        new(v.Id, v.UserId, v.User?.Username ?? string.Empty, v.User?.AvatarPath,
            v.Title, v.Description, v.Status.ToString(),
            v.ThumbnailPath, v.DurationSeconds, v.ViewCount,
            v.Resolutions, v.Tags, v.CreatedAt);

    private record UploadMeta(string FileName, long FileSize, int TotalChunks);
}