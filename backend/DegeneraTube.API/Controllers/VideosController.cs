using DegeneraTube.Application.Videos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DegeneraTube.API.Controllers;

public class VideosController(IVideoService videos) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default) =>
        FromResult(await videos.GetPagedAsync(page, pageSize, search, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        FromResult(await videos.GetByIdAsync(id, ct));

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await videos.GetByUserAsync(userId, page, pageSize, ct));

    [Authorize]
    [HttpPost]
    [RequestSizeLimit(4L * 1024 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] VideoUploadRequest request,
        [FromForm] IFormFile file,
        CancellationToken ct)
    {
        if (file == null)
            return BadRequest("File is required");

        await using var stream = file.OpenReadStream();

        return FromResult(
            await videos.CreateAsync(
                CurrentUserId,
                request,
                stream,
                file.FileName,
                ct));
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, VideoUpdateRequest request, CancellationToken ct) =>
        FromResult(await videos.UpdateAsync(id, CurrentUserId, request, ct));

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        FromResult(await videos.DeleteAsync(id, CurrentUserId, ct));

    [HttpPost("{id:guid}/view")]
    public async Task<IActionResult> RegisterView(Guid id, CancellationToken ct) =>
        FromResult(await videos.RegisterViewAsync(id, ct));

    [HttpGet("{id:guid}/stream")]
    public IActionResult Stream(Guid id)
    {
        var path = videos.GetStreamPath(id);

        if (!System.IO.File.Exists(path))
            return NotFound();

        return PhysicalFile(path, "application/vnd.apple.mpegurl", enableRangeProcessing: true);
    }

    [HttpGet("{id:guid}/stream/{quality}/{segment}")]
    public IActionResult StreamSegment(Guid id, string quality, string segment)
    {
        var dir = System.IO.Path.GetDirectoryName(videos.GetStreamPath(id))!;
        var path = System.IO.Path.Combine(dir, quality, segment);

        if (!System.IO.File.Exists(path))
            return NotFound();

        return PhysicalFile(path, "video/mp2t", enableRangeProcessing: true);
    }
}