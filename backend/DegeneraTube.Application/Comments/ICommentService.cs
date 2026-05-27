using DegeneraTube.Shared;

namespace DegeneraTube.Application.Comments;

public interface ICommentService
{
    Task<Result<CommentPagedResponse>> GetByVideoAsync(Guid videoId, int page, int pageSize, CancellationToken ct = default);
    Task<Result<List<CommentDto>>> GetRepliesAsync(Guid parentId, CancellationToken ct = default);
    Task<Result<CommentDto>> CreateAsync(Guid userId, Guid videoId, CreateCommentRequest request, CancellationToken ct = default);
    Task<Result<CommentDto>> UpdateAsync(Guid commentId, Guid requesterId, UpdateCommentRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid commentId, Guid requesterId, CancellationToken ct = default);
}