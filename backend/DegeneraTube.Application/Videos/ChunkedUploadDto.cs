namespace DegeneraTube.Application.Videos;

public record InitUploadResponse(Guid UploadId);

public record UploadChunkResponse(int ReceivedChunks, int TotalChunks, bool IsComplete);

public record InitUploadRequest(
    string Title,
    string? Description,
    List<string>? Tags,
    string FileName,
    long FileSize,
    int TotalChunks);
    