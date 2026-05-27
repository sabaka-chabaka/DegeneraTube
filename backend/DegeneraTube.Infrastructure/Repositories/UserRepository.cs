using DegeneraTube.Domain.Entities;
using DegeneraTube.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DegeneraTube.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : BaseRepository<User>(db), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        await Set.AnyAsync(u => u.Email == email, ct);

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default) =>
        await Set.AnyAsync(u => u.Username == username, ct);
}