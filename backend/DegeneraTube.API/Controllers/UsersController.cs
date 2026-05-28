using DegeneraTube.Application.Users;
using DegeneraTube.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DegeneraTube.API.Controllers;

public class UsersController(IUserService users, IFileStorage storage) : BaseController
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProfile(Guid id, CancellationToken ct) =>
        FromResult(await users.GetProfileAsync(id, ct));

    [HttpGet("{id:guid}/avatar")]
    public async Task<IActionResult> GetAvatar(Guid id, CancellationToken ct)
    {
        try
        {
            var path = await users.GetAvatarPathAsync(id, ct);
            if (string.IsNullOrEmpty(path)) return NotFound();

            var fullPath = storage.GetFullPath(path);
            if (!System.IO.File.Exists(fullPath)) return NotFound();

            var extension = System.IO.Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "image/jpeg"
            };

            return PhysicalFile(fullPath, contentType);
        }
        catch
        {
            return NotFound();
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct) =>
        FromResult(await users.GetByIdAsync(CurrentUserId, ct));

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request, CancellationToken ct) =>
        FromResult(await users.UpdateProfileAsync(CurrentUserId, request, ct));

    [Authorize]
    [HttpPost("{id:guid}/subscribe")]
    public async Task<IActionResult> Subscribe(Guid id, CancellationToken ct) =>
        FromResult(await users.SubscribeAsync(CurrentUserId, id, ct));

    [Authorize]
    [HttpDelete("{id:guid}/subscribe")]
    public async Task<IActionResult> Unsubscribe(Guid id, CancellationToken ct) =>
        FromResult(await users.UnsubscribeAsync(CurrentUserId, id, ct));

    [Authorize]
    [HttpPost("{id:guid}/ban")]
    public async Task<IActionResult> Ban(Guid id, CancellationToken ct) =>
        FromResult(await users.BanAsync(id, ct));
}