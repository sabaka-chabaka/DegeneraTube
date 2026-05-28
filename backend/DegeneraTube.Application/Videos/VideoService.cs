using DegeneraTube.Domain.Entities;
using DegeneraTube.Domain.Enums;
using DegeneraTube.Infrastructure.Processing;
using DegeneraTube.Infrastructure.Repositories;
using DegeneraTube.Infrastructure.Storage;
using DegeneraTube.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace DegeneraTube.Application.Videos;

public class VideoService(
    IServiceScopeFactory scopeFactory,
    IFileStorage storage) : IVideoService
{
    public async Task<Result<VideoDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
        var video = await videos.GetByIdWithUserAsync(id, ct);

        if (video is null)
            return Result.Failure<VideoDto>("Video not found.");

        if (!video.IsReady())
            return Result.Failure<VideoDto>("Video is not available yet.");

        return Result.Success(ToDto(video));
    }

    public async Task<Result<VideoPagedResponse>> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
        var paged = await videos.GetPagedAsync(page, pageSize, search, ct);
        return Result.Success(ToPagedResponse(paged));
    }

    public async Task<Result<VideoPagedResponse>> GetByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
        var paged = await videos.GetByUserIdAsync(userId, page, pageSize, ct);
        return Result.Success(ToPagedResponse(paged));
    }

    public async Task<Result<VideoDto>> CreateAsync(
        Guid userId,
        VideoUploadRequest request,
        Stream videoStream,
        string fileName,
        CancellationToken ct = default)
    {
        var video = new Video
        {
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            Tags = request.Tags ?? [],
            Status = VideoStatus.Processing
        };

        using (var scope = scopeFactory.CreateScope())
        {
            var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
            await videos.AddAsync(video, ct);
            await videos.SaveAsync(ct);
        }

        _ = ProcessVideoAsync(video.Id, videoStream, fileName);

        using (var scope = scopeFactory.CreateScope())
        {
            var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
            var saved = await videos.GetByIdWithUserAsync(video.Id, ct);
            return Result.Success(ToDto(saved!));
        }
    }

    public async Task<Result<VideoDto>> UpdateAsync(
        Guid videoId, Guid requesterId, VideoUpdateRequest request, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
        var video = await videos.GetByIdWithUserAsync(videoId, ct);

        if (video is null)
            return Result.Failure<VideoDto>("Video not found.");

        if (!video.IsOwnedBy(requesterId))
            return Result.Failure<VideoDto>("Access denied.");

        if (request.Title is not null) video.Title = request.Title;
        if (request.Description is not null) video.Description = request.Description;
        if (request.Tags is not null) video.Tags = request.Tags;

        videos.Update(video);
        await videos.SaveAsync(ct);

        return Result.Success(ToDto(video));
    }

    public async Task<Result> DeleteAsync(Guid videoId, Guid requesterId, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
        var video = await videos.GetByIdAsync(videoId, ct);

        if (video is null)
            return Result.Failure("Video not found.");

        if (!video.IsOwnedBy(requesterId))
            return Result.Failure("Access denied.");

        if (video.HlsPath is not null)
            await storage.DeleteAsync(video.HlsPath, ct);

        if (video.ThumbnailPath is not null)
            await storage.DeleteAsync(video.ThumbnailPath, ct);

        videos.Remove(video);
        await videos.SaveAsync(ct);
        return Result.Success();
    }

    public async Task<Result> RegisterViewAsync(Guid videoId, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
        await videos.IncrementViewCountAsync(videoId, ct);
        return Result.Success();
    }

    public string GetStreamPath(Guid videoId) =>
        storage.GetFullPath(Path.Combine("hls", videoId.ToString(), "master.m3u8"));

    private async Task ProcessVideoAsync(Guid videoId, Stream stream, string fileName)
    {
        try
        {
            var rawFolder = Path.Combine("raw", videoId.ToString());
            var rawPath = await storage.SaveAsync(stream, rawFolder, fileName);
            var fullRawPath = storage.GetFullPath(rawPath);

            using var scope = scopeFactory.CreateScope();
            var ffmpeg = scope.ServiceProvider.GetRequiredService<FfmpegService>();
            var thumbnails = scope.ServiceProvider.GetRequiredService<ThumbnailService>();
            var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();

            var hlsOutputDir = storage.GetFullPath(Path.Combine("hls", videoId.ToString()));
            var hlsResult = await ffmpeg.TranscodeToHlsAsync(fullRawPath, hlsOutputDir);

            var thumbDir = storage.GetFullPath(Path.Combine("thumbnails", videoId.ToString()));
            var probe = await ffmpeg.ProbeAsync(fullRawPath);
            var thumbPath = await thumbnails.ExtractBestAsync(fullRawPath, thumbDir, probe.DurationSeconds);

            var video = await videos.GetByIdAsync(videoId);

            if (video is null) return;

            video.Status = VideoStatus.Ready;
            video.HlsPath = Path.Combine("hls", videoId.ToString());
            video.ThumbnailPath = Path.GetRelativePath(storage.GetFullPath(""), thumbPath);
            video.DurationSeconds = probe.DurationSeconds;
            video.Resolutions = hlsResult.Resolutions;

            videos.Update(video);
            await videos.SaveAsync();

            await storage.DeleteAsync(rawPath);
        }
        catch
        {
            using var scope = scopeFactory.CreateScope();
            var videos = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
            var video = await videos.GetByIdAsync(videoId);
            if (video is null) return;
            video.Status = VideoStatus.Failed;
            videos.Update(video);
            await videos.SaveAsync();
        }
    }

    private static VideoDto ToDto(Video v) =>
        new(v.Id, v.UserId, v.User.Username, v.User.AvatarPath,
            v.Title, v.Description, v.Status.ToString(),
            v.ThumbnailPath, v.DurationSeconds, v.ViewCount,
            v.Resolutions, v.Tags, v.CreatedAt);

    private static VideoPagedResponse ToPagedResponse(PagedList<Video> paged) =>
        new(paged.Items.Select(ToDto).ToList(),
            paged.Page, paged.PageSize, paged.TotalCount, paged.TotalPages);
}