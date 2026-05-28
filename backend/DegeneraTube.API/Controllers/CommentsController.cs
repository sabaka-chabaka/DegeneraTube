using DegeneraTube.Application.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DegeneraTube.API.Controllers;

public class CommentsController(ICommentService comments) : BaseController
{
    [HttpGet("video/{videoId:guid}")]
    public async Task<IActionResult> GetByVideo(
        Guid videoId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await comments.GetByVideoAsync(videoId, page, pageSize, ct));

    [HttpGet("{id:guid}/replies")]
    public async Task<IActionResult> GetReplies(Guid id, CancellationToken ct) =>
        FromResult(await comments.GetRepliesAsync(id, ct));

    [Authorize]
    [HttpPost("video/{videoId:guid}")]
    public async Task<IActionResult> Create(
        Guid videoId, CreateCommentRequest request, CancellationToken ct) =>
        FromResult(await comments.CreateAsync(CurrentUserId, videoId, request, ct));

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, UpdateCommentRequest request, CancellationToken ct) =>
        FromResult(await comments.UpdateAsync(id, CurrentUserId, request, ct));

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        FromResult(await comments.DeleteAsync(id, CurrentUserId, ct));
}