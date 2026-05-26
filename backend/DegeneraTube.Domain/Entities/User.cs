namespace DegeneraTube.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
    public bool IsBanned { get; set; }
 
    public ICollection<Video> Videos { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<Subscription> Subscribers { get; set; } = [];
}
