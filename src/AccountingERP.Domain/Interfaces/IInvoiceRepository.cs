namespace AccountingERP.Domain.Interfaces;

using AccountingERP.Domain.Aggregates.Invoice;
using AccountingERP.Domain.ValueObjects;

public interface IInvoiceRepository : IRepository<Invoice, int>
{
    Task<Invoice?>           GetByNumberAsync(int tenantId, string number, CancellationToken ct);
    Task<IEnumerable<Invoice>> GetAllAsync(int tenantId, InvoiceFilter filter, CancellationToken ct);
    Task<InvoiceSummaryData> GetSummaryAsync(int tenantId, CancellationToken ct);
    Task<bool>               NumberExistsAsync(int tenantId, string number, CancellationToken ct);
}

public record InvoiceFilter(int Page, int PageSize, string? Status, int? ClientId, string? Search);
public record InvoiceSummaryData(int TotalCount, Money TotalAmount, Money PaidAmount, Money OverdueAmount);
