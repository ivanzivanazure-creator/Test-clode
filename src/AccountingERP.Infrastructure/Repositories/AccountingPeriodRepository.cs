namespace AccountingERP.Infrastructure.Repositories;

using AccountingERP.Domain.Aggregates.AccountingPeriod;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class AccountingPeriodRepository : IAccountingPeriodRepository
{
    private readonly AppDbContext _db;

    public AccountingPeriodRepository(AppDbContext db) => _db = db;

    public async Task<AccountingPeriod?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.AccountingPeriods.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<AccountingPeriod> GetByIdOrThrowAsync(int id, CancellationToken ct = default)
        => await GetByIdAsync(id, ct)
           ?? throw new NotFoundException(nameof(AccountingPeriod), id);

    public void Add(AccountingPeriod entity)    => _db.AccountingPeriods.Add(entity);
    public void Remove(AccountingPeriod entity) => _db.AccountingPeriods.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task<IEnumerable<AccountingPeriod>> GetByYearAsync(
        int tenantId, int year, CancellationToken ct)
        => await _db.AccountingPeriods
                    .AsNoTracking()
                    .Where(p => p.TenantId == tenantId && p.Year == year)
                    .OrderBy(p => p.Month)
                    .ToListAsync(ct);

    public async Task<AccountingPeriod?> GetAsync(
        int tenantId, int month, int year, CancellationToken ct)
        => await _db.AccountingPeriods
                    .FirstOrDefaultAsync(
                        p => p.TenantId == tenantId && p.Month == month && p.Year == year, ct);
}
