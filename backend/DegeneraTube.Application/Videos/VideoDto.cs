namespace DegeneraTube.Application.Videos;

public record VideoDto(
    Guid Id,
    Guid UserId,
    string AuthorUsername,
    string? AuthorAvatarPath,
    string Title,
    string? Description,
    string Status,
    string? ThumbnailPath,
    int DurationSeconds,
    long ViewCount,
    List<int> Resolutions,
    List<string> Tags,
    DateTime CreatedAt);

public record VideoUploadRequest(string Title, string? Description, List<string>? Tags);

public record VideoUpdateRequest(string? Title, string? Description, List<string>? Tags);

public record VideoPagedResponse(
    List<VideoDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);