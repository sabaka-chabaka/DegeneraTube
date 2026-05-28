using DegeneraTube.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DegeneraTube.API.Controllers;

public class UsersController(IUserService users) : BaseController
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProfile(Guid id, CancellationToken ct) =>
        FromResult(await users.GetProfileAsync(id, ct));

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