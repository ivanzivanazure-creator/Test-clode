namespace AccountingERP.Infrastructure.Repositories;

using AccountingERP.Domain.Aggregates.Client;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _db;

    public ClientRepository(AppDbContext db) => _db = db;

    public async Task<Client?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Clients.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Client> GetByIdOrThrowAsync(int id, CancellationToken ct = default)
        => await GetByIdAsync(id, ct)
           ?? throw new NotFoundException(nameof(Client), id);

    public void Add(Client entity)    => _db.Clients.Add(entity);
    public void Remove(Client entity) => _db.Clients.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task<IEnumerable<Client>> GetAllAsync(
        int tenantId, string? search, CancellationToken ct)
    {
        var query = _db.Clients
                       .AsNoTracking()
                       .Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(s) ||
                (c.PIB != null && c.PIB.Contains(s)) ||
                (c.City != null && c.City.ToLower().Contains(s)));
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> PIBExistsAsync(int tenantId, string pib, CancellationToken ct)
        => await _db.Clients.AnyAsync(
               c => c.TenantId == tenantId && c.PIB == pib, ct);
}
