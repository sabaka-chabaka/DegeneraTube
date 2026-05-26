using DegeneraTube.Domain.Enums;

namespace DegeneraTube.Domain.Entities;

public class Video : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public VideoStatus Status { get; set; } = VideoStatus.Processing;
    public string? HlsPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public int DurationSeconds { get; set; }
    public long ViewCount { get; set; }
    public List<int> Resolutions { get; set; } = [];
    public List<string> Tags { get; set; } = [];

    public User User { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = [];

    public bool IsOwnedBy(Guid userId) => UserId == userId;
    public bool IsReady() => Status == VideoStatus.Ready;
}