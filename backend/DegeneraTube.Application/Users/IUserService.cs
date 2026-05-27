using DegeneraTube.Shared;

namespace DegeneraTube.Application.Users;

public interface IUserService
{
    Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<UserProfileDto>> GetProfileAsync(Guid id, CancellationToken ct = default);
    Task<Result<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<Result> SubscribeAsync(Guid subscriberId, Guid channelId, CancellationToken ct = default);
    Task<Result> UnsubscribeAsync(Guid subscriberId, Guid channelId, CancellationToken ct = default);
    Task<Result> BanAsync(Guid userId, CancellationToken ct = default);
}