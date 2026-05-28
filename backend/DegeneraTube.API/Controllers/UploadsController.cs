using DegeneraTube.Application.Videos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DegeneraTube.API.Controllers;

[Authorize]
public class UploadsController(IChunkedUploadService chunked) : BaseController
{
    private const long ChunkSizeLimit = 10L * 1024 * 1024;

    [HttpPost("init")]
    public async Task<IActionResult> Init(
        [FromBody] InitUploadRequest request,
        CancellationToken ct)
    {
        if (request.TotalChunks <= 0)
            return BadRequest(new { error = "totalChunks must be > 0" });
        if (string.IsNullOrWhiteSpace(request.FileName))
            return BadRequest(new { error = "fileName is required" });

        return FromResult(await chunked.InitAsync(CurrentUserId, request, ct));
    }
    
    [HttpPut("{uploadId:guid}/chunk/{chunkIndex:int}")]
    [RequestSizeLimit(ChunkSizeLimit + 512 * 1024)]
    public async Task<IActionResult> UploadChunk(
        Guid uploadId,
        int chunkIndex,
        [FromHeader(Name = "X-Total-Chunks")] int totalChunks,
        CancellationToken ct)
    {
        if (!Request.ContentLength.HasValue || Request.ContentLength <= 0)
            return BadRequest(new { error = "Empty chunk body." });

        if (chunkIndex < 0)
            return BadRequest(new { error = "chunkIndex must be >= 0" });

        if (totalChunks <= 0)
            return BadRequest(new { error = "X-Total-Chunks header must be > 0" });

        return FromResult(await chunked.SaveChunkAsync(
            uploadId,
            CurrentUserId,
            chunkIndex,
            totalChunks,
            Request.Body,
            ct));
    }
    
    [HttpPost("{uploadId:guid}/finalize")]
    public async Task<IActionResult> Finalize(Guid uploadId, CancellationToken ct) =>
        FromResult(await chunked.FinalizeAsync(uploadId, CurrentUserId, ct));
}