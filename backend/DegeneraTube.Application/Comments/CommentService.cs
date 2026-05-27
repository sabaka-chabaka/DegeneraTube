using DegeneraTube.Domain.Entities;
using DegeneraTube.Infrastructure.Repositories;
using DegeneraTube.Shared;

namespace DegeneraTube.Application.Comments;

public class CommentService(
    ICommentRepository comments,
    IVideoRepository videos) : ICommentService
{
    public async Task<Result<CommentPagedResponse>> GetByVideoAsync(
        Guid videoId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await comments.GetByVideoIdAsync(videoId, page, pageSize, ct);

        var dtos = new List<CommentDto>();
        foreach (var c in paged.Items)
        {
            var replyCount = (await comments.GetRepliesAsync(c.Id, ct)).Count;
            dtos.Add(ToDto(c, replyCount));
        }

        return Result.Success(new CommentPagedResponse(
            dtos, paged.Page, paged.PageSize, paged.TotalCount, paged.TotalPages));
    }

    public async Task<Result<List<CommentDto>>> GetRepliesAsync(
        Guid parentId, CancellationToken ct = default)
    {
        var replies = await comments.GetRepliesAsync(parentId, ct);
        return Result.Success(replies.Select(c => ToDto(c, 0)).ToList());
    }

    public async Task<Result<CommentDto>> CreateAsync(
        Guid userId, Guid videoId, CreateCommentRequest request, CancellationToken ct = default)
    {
        var video = await videos.GetByIdAsync(videoId, ct);

        if (video is null || !video.IsReady())
            return Result.Failure<CommentDto>("Video not found or not available.");

        if (request.ParentId is not null)
        {
            var parent = await comments.GetByIdAsync(request.ParentId.Value, ct);
            if (parent is null || parent.VideoId != videoId)
                return Result.Failure<CommentDto>("Parent comment not found.");

            if (parent.IsReply())
                return Result.Failure<CommentDto>("Cannot reply to a reply.");
        }

        var comment = new Comment
        {
            UserId = userId,
            VideoId = videoId,
            ParentId = request.ParentId,
            Body = request.Body
        };

        await comments.AddAsync(comment, ct);
        await comments.SaveAsync(ct);

        var saved = await comments.GetByIdWithUserAsync(comment.Id, ct);
        return Result.Success(ToDto(saved!, 0));
    }

    public async Task<Result<CommentDto>> UpdateAsync(
        Guid commentId, Guid requesterId, UpdateCommentRequest request, CancellationToken ct = default)
    {
        var comment = await comments.GetByIdWithUserAsync(commentId, ct);

        if (comment is null || comment.IsDeleted)
            return Result.Failure<CommentDto>("Comment not found.");

        if (!comment.IsOwnedBy(requesterId))
            return Result.Failure<CommentDto>("Access denied.");

        comment.Body = request.Body;
        comments.Update(comment);
        await comments.SaveAsync(ct);

        return Result.Success(ToDto(comment, 0));
    }

    public async Task<Result> DeleteAsync(
        Guid commentId, Guid requesterId, CancellationToken ct = default)
    {
        var comment = await comments.GetByIdAsync(commentId, ct);

        if (comment is null)
            return Result.Failure("Comment not found.");

        if (!comment.IsOwnedBy(requesterId))
            return Result.Failure("Access denied.");

        comment.IsDeleted = true;
        comment.Body = string.Empty;
        comments.Update(comment);
        await comments.SaveAsync(ct);

        return Result.Success();
    }

    private static CommentDto ToDto(Comment c, int replyCount) =>
        new(c.Id, c.VideoId, c.UserId,
            c.User.Username, c.User.AvatarPath,
            c.ParentId, c.Body, c.CreatedAt, replyCount);
}