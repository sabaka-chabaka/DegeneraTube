using DegeneraTube.Domain.Entities;
using DegeneraTube.Infrastructure.Repositories;
using DegeneraTube.Shared;
using System.Security.Cryptography;

namespace DegeneraTube.Application.Auth;

public class AuthService(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    TokenService tokenService) : IAuthService
{
    public async Task<Result<TokenResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await users.ExistsByEmailAsync(request.Email, ct))
            return Result.Failure<TokenResponse>("Email already taken.");

        if (await users.ExistsByUsernameAsync(request.Username, ct))
            return Result.Failure<TokenResponse>("Username already taken.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = HashPassword(request.Password)
        };

        await users.AddAsync(user, ct);
        await users.SaveAsync(ct);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);

        if (user is null || !VerifyPassword(request.Password, user.PasswordHash))
            return Result.Failure<TokenResponse>("Invalid credentials.");

        if (user.IsBanned)
            return Result.Failure<TokenResponse>("Account is banned.");

        return await IssueTokensAsync(user, ct);
    }

    public async Task<Result<TokenResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
    {
        var token = await refreshTokens.GetByTokenAsync(request.RefreshToken, ct);

        if (token is null || !token.IsActive)
            return Result.Failure<TokenResponse>("Invalid or expired refresh token.");

        token.IsRevoked = true;
        refreshTokens.Update(token);
        await refreshTokens.SaveAsync(ct);

        return await IssueTokensAsync(token.User, ct);
    }

    public async Task<Result> RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await refreshTokens.GetByTokenAsync(refreshToken, ct);

        if (token is null)
            return Result.Failure("Token not found.");

        token.IsRevoked = true;
        refreshTokens.Update(token);
        await refreshTokens.SaveAsync(ct);

        return Result.Success();
    }

    private async Task<Result<TokenResponse>> IssueTokensAsync(User user, CancellationToken ct)
    {
        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user.Id, user.Username);
        var rawRefresh = tokenService.GenerateRefreshToken();

        await refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = rawRefresh,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        }, ct);

        await refreshTokens.SaveAsync(ct);

        return Result.Success(new TokenResponse(accessToken, rawRefresh, expiresAt));
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}