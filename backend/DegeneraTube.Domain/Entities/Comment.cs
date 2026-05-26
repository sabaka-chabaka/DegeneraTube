namespace DegeneraTube.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid VideoId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentId { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public Video Video { get; set; } = null!;
    public User User { get; set; } = null!;
    public Comment? Parent { get; set; }
    public ICollection<Comment> Replies { get; set; } = [];

    public bool IsOwnedBy(Guid userId) => UserId == userId;
    public bool IsReply() => ParentId is not null;
}