using DegeneraTube.Domain.Entities;
using DegeneraTube.Infrastructure.Repositories;
using DegeneraTube.Shared;

namespace DegeneraTube.Application.Users;

public class UserService(
    IUserRepository users,
    ISubscriptionRepository subscriptions,
    IVideoRepository videos) : IUserService
{
    public async Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(id, ct);

        if (user is null)
            return Result.Failure<UserDto>($"User {id} not found.");

        return Result.Success(ToDto(user));
    }

    public async Task<Result<UserProfileDto>> GetProfileAsync(Guid id, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(id, ct);

        if (user is null)
            return Result.Failure<UserProfileDto>($"User {id} not found.");

        var videoCount = await videos.CountByUserAsync(id, ct);
        var subscriberCount = await subscriptions.CountSubscribersAsync(id, ct);

        return Result.Success(new UserProfileDto(
            user.Id,
            user.Username,
            user.AvatarPath,
            videoCount,
            subscriberCount,
            user.CreatedAt));
    }

    public async Task<Result<UserDto>> UpdateProfileAsync(
        Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct);

        if (user is null)
            return Result.Failure<UserDto>("User not found.");

        if (request.Username is not null)
        {
            if (await users.ExistsByUsernameAsync(request.Username, ct))
                return Result.Failure<UserDto>("Username already taken.");

            user.Username = request.Username;
        }

        if (request.AvatarPath is not null)
            user.AvatarPath = request.AvatarPath;

        users.Update(user);
        await users.SaveAsync(ct);

        return Result.Success(ToDto(user));
    }

    public async Task<Result> SubscribeAsync(Guid subscriberId, Guid channelId, CancellationToken ct = default)
    {
        if (subscriberId == channelId)
            return Result.Failure("Cannot subscribe to yourself.");

        var exists = await subscriptions.ExistsAsync(subscriberId, channelId, ct);

        if (exists)
            return Result.Failure("Already subscribed.");

        await subscriptions.AddAsync(new Subscription
        {
            SubscriberId = subscriberId,
            ChannelId = channelId
        }, ct);

        await subscriptions.SaveAsync(ct);
        return Result.Success();
    }

    public async Task<Result> UnsubscribeAsync(Guid subscriberId, Guid channelId, CancellationToken ct = default)
    {
        var subscription = await subscriptions.GetAsync(subscriberId, channelId, ct);

        if (subscription is null)
            return Result.Failure("Subscription not found.");

        subscriptions.Remove(subscription);
        await subscriptions.SaveAsync(ct);
        return Result.Success();
    }

    public async Task<Result> BanAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct);

        if (user is null)
            return Result.Failure("User not found.");

        user.IsBanned = true;
        users.Update(user);
        await users.SaveAsync(ct);
        return Result.Success();
    }

    private static UserDto ToDto(Domain.Entities.User u) =>
        new(u.Id, u.Username, u.Email, u.AvatarPath, u.CreatedAt);
}