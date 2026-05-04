namespace AccountingERP.Infrastructure.Repositories;

using AccountingERP.Domain.Aggregates.User;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User> GetByIdOrThrowAsync(int id, CancellationToken ct = default)
        => await GetByIdAsync(id, ct)
           ?? throw new NotFoundException(nameof(User), id);

    public void Add(User entity)    => _db.Users.Add(entity);
    public void Remove(User entity) => _db.Users.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task<User?> GetByUsernameAsync(int tenantId, string username, CancellationToken ct)
        => await _db.Users.FirstOrDefaultAsync(
               u => u.TenantId == tenantId && u.Username == username, ct);

    public async Task<IEnumerable<User>> GetAllAsync(int tenantId, CancellationToken ct)
        => await _db.Users
                    .AsNoTracking()
                    .Where(u => u.TenantId == tenantId)
                    .OrderBy(u => u.FullName)
                    .ToListAsync(ct);
}
