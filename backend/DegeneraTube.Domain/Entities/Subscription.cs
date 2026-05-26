namespace DegeneraTube.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid SubscriberId { get; set; }
    public Guid ChannelId { get; set; }
    public bool NotificationsEnabled { get; set; } = true;

    public User Subscriber { get; set; } = null!;
    public User Channel { get; set; } = null!;
}