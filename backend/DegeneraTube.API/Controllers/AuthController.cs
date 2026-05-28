using DegeneraTube.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DegeneraTube.API.Controllers;

public class AuthController(IAuthService auth) : BaseController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct) =>
        FromResult(await auth.RegisterAsync(request, ct));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct) =>
        FromResult(await auth.LoginAsync(request, ct));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken ct) =>
        FromResult(await auth.RefreshAsync(request, ct));

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken ct) =>
        FromResult(await auth.RevokeAsync(request.RefreshToken, ct));
}