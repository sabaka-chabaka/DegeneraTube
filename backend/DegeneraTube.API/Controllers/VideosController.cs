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

    [HttpGet("{id:guid}/master.m3u8")]
    public async Task<IActionResult> Stream(Guid id, CancellationToken ct)
    {
        var path = await videos.GetStreamPathAsync(id, ct);

        if (path == null)
            return NotFound();

        path = System.IO.Path.GetFullPath(path);

        if (!System.IO.File.Exists(path))
            return NotFound();

        return PhysicalFile(path, "application/vnd.apple.mpegurl");
    }

    [HttpGet("{id:guid}/{quality}/index.m3u8")]
    public async Task<IActionResult> StreamPlaylist(Guid id, string quality, CancellationToken ct)
    {
        var masterPath = await videos.GetStreamPathAsync(id, ct);
        if (masterPath == null) return NotFound();

        var dir = System.IO.Path.GetDirectoryName(masterPath)!;
        var path = System.IO.Path.Combine(dir, quality, "index.m3u8");

        path = System.IO.Path.GetFullPath(path);

        if (!System.IO.File.Exists(path))
            return NotFound();

        return PhysicalFile(path, "application/vnd.apple.mpegurl");
    }

    [HttpGet("{id:guid}/{quality}/{segment}")]
    public async Task<IActionResult> StreamSegment(Guid id, string quality, string segment, CancellationToken ct)
    {
        var masterPath = await videos.GetStreamPathAsync(id, ct);
        if (masterPath == null) return NotFound();

        var dir = System.IO.Path.GetDirectoryName(masterPath)!;
        var path = System.IO.Path.Combine(dir, quality, segment);

        path = System.IO.Path.GetFullPath(path);

        if (!System.IO.File.Exists(path))
            return NotFound();

        var contentType = segment.EndsWith(".m3u8") ? "application/vnd.apple.mpegurl" : "video/mp2t";
        return PhysicalFile(path, contentType);
    }

    [HttpGet("{id:guid}/thumbnail")]
    public async Task<IActionResult> GetThumbnail(Guid id, CancellationToken ct)
    {
        try
        {
            var path = await videos.GetThumbnailPathAsync(id, ct);
            if (string.IsNullOrEmpty(path))
                return NotFound();

            if (!System.IO.File.Exists(path))
                return NotFound();

            var extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "image/jpeg"
            };

            return PhysicalFile(path, contentType);
        }
        catch
        {
            return NotFound();
        }
    }
}