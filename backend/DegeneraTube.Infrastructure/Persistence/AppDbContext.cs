using DegeneraTube.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DegeneraTube.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
 
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }
 
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
 
        return base.SaveChangesAsync(cancellationToken);
    }
}
