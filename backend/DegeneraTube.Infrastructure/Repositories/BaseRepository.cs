using DegeneraTube.Domain.Entities;
using DegeneraTube.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DegeneraTube.Infrastructure.Repositories;

public abstract class BaseRepository<T>(AppDbContext db) : IRepository<T>
    where T : BaseEntity
{
    protected readonly DbSet<T> Set = db.Set<T>();
    protected readonly AppDbContext Db = db;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await Set.FindAsync([id], ct);

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public void Update(T entity) =>
        Set.Update(entity);

    public void Remove(T entity) =>
        Set.Remove(entity);

    public Task SaveAsync(CancellationToken ct = default) =>
        Db.SaveChangesAsync(ct);
}