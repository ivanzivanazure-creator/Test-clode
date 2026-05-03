namespace AccountingERP.Infrastructure.Repositories;

using AccountingERP.Domain.Aggregates.Invoice;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Domain.ValueObjects;
using AccountingERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _db;

    public InvoiceRepository(AppDbContext db) => _db = db;

    public async Task<Invoice?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Invoices
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<Invoice> GetByIdOrThrowAsync(int id, CancellationToken ct = default)
        => await GetByIdAsync(id, ct)
           ?? throw new NotFoundException(nameof(Invoice), id);

    public void Add(Invoice entity)    => _db.Invoices.Add(entity);
    public void Remove(Invoice entity) => _db.Invoices.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task<Invoice?> GetByNumberAsync(int tenantId, string number, CancellationToken ct)
        => await _db.Invoices
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(
                        i => i.TenantId == new TenantId(tenantId) && i.Number == number, ct);

    public async Task<IEnumerable<Invoice>> GetAllAsync(
        int tenantId, InvoiceFilter filter, CancellationToken ct)
    {
        var query = _db.Invoices
                       .Include(i => i.Items)
                       .AsNoTracking()
                       .Where(i => i.TenantId == new TenantId(tenantId));

        if (!string.IsNullOrWhiteSpace(filter.Status)
            && Enum.TryParse<InvoiceStatus>(filter.Status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(i => i.Status == parsedStatus);
        }

        if (filter.ClientId.HasValue)
            query = query.Where(i => i.ClientId == filter.ClientId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(i => i.Number.ToLower().Contains(search));
        }

        var page     = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        return await query
            .OrderByDescending(i => i.IssueDate)
            .ThenByDescending(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<InvoiceSummaryData> GetSummaryAsync(int tenantId, CancellationToken ct)
    {
        var tid = new TenantId(tenantId);

        var invoices = await _db.Invoices
                                .Include(i => i.Items)
                                .AsNoTracking()
                                .Where(i => i.TenantId == tid)
                                .ToListAsync(ct);

        var totalCount   = invoices.Count;
        var totalAmount  = invoices.Sum(i => i.TotalAmount.Amount);
        var paidAmount   = invoices
            .Where(i => i.Status == InvoiceStatus.Plaćena)
            .Sum(i => i.TotalAmount.Amount);
        var overdueAmount = invoices
            .Where(i => i.Status == InvoiceStatus.Dospela)
            .Sum(i => i.TotalAmount.Amount);

        return new InvoiceSummaryData(
            TotalCount:    totalCount,
            TotalAmount:   Money.FromRSD(totalAmount),
            PaidAmount:    Money.FromRSD(paidAmount),
            OverdueAmount: Money.FromRSD(overdueAmount));
    }

    public async Task<bool> NumberExistsAsync(int tenantId, string number, CancellationToken ct)
        => await _db.Invoices.AnyAsync(
               i => i.TenantId == new TenantId(tenantId) && i.Number == number, ct);
}
