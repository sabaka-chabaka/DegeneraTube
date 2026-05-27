using DegeneraTube.Shared;

namespace DegeneraTube.Application.Auth;

public interface IAuthService
{
    Task<Result<TokenResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<TokenResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default);
    Task<Result> RevokeAsync(string refreshToken, CancellationToken ct = default);
}