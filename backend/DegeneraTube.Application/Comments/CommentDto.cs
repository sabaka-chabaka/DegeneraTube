namespace DegeneraTube.Application.Comments;

public record CommentDto(
    Guid Id,
    Guid VideoId,
    Guid UserId,
    string AuthorUsername,
    string? AuthorAvatarPath,
    Guid? ParentId,
    string Body,
    DateTime CreatedAt,
    int ReplyCount);

public record CreateCommentRequest(string Body, Guid? ParentId);

public record UpdateCommentRequest(string Body);

public record CommentPagedResponse(
    List<CommentDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);