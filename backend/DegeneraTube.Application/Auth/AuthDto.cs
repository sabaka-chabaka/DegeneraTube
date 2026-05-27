namespace DegeneraTube.Application.Auth;

public record RegisterRequest(string Username, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public record RefreshRequest(string RefreshToken);