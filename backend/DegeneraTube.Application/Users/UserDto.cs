namespace DegeneraTube.Application.Users;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string? AvatarPath,
    DateTime CreatedAt);

public record UserProfileDto(
    Guid Id,
    string Username,
    string? AvatarPath,
    int VideoCount,
    int SubscriberCount,
    DateTime CreatedAt);

public record UpdateProfileRequest(string? Username, string? AvatarPath);