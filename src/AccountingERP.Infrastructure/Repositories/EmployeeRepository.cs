namespace AccountingERP.Infrastructure.Repositories;

using AccountingERP.Domain.Aggregates.Employee;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Domain.ValueObjects;
using AccountingERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db) => _db = db;

    // ── IRepository<Employee, int> ────────────────────────────────────────────

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Employees
                    .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Employee> GetByIdOrThrowAsync(int id, CancellationToken ct = default)
        => await GetByIdAsync(id, ct)
           ?? throw new NotFoundException(nameof(Employee), id);

    public void Add(Employee entity)    => _db.Employees.Add(entity);
    public void Remove(Employee entity) => _db.Employees.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    // ── IEmployeeRepository ───────────────────────────────────────────────────

    /// <summary>
    /// Returns all active (non-terminated, non-pseudonymized) employees for the tenant.
    /// Ordered by last name then first name for consistent UI display.
    /// </summary>
    public async Task<IEnumerable<Employee>> GetActiveAsync(int tenantId, CancellationToken ct)
    {
        var tid = new TenantId(tenantId);

        return await _db.Employees
                        .AsNoTracking()
                        .Where(e => e.TenantId == tid && e.IsActive)
                        .OrderBy(e => e.LastName)
                        .ThenBy(e => e.FirstName)
                        .ToListAsync(ct);
    }

    /// <summary>
    /// Looks up an employee by the SHA-256 hash of their JMBG (national ID).
    /// The hash index on (TenantId, JMBGHashSha256) makes this a single index seek.
    /// Returns null when no match is found.
    /// </summary>
    public async Task<Employee?> GetByJMBGHashAsync(
        int tenantId, string jmbgHash, CancellationToken ct)
    {
        var tid = new TenantId(tenantId);

        return await _db.Employees
                        .FirstOrDefaultAsync(
                            e => e.TenantId       == tid
                              && e.JMBGHashSha256  == jmbgHash,
                            ct);
    }
}
