namespace AccountingERP.Application.Queries.Reports;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Aggregates.Invoice;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Domain.ValueObjects;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Builds a VAT return (PP PDV obrazac) for the specified month and year.
/// Queries all issued/paid/overdue invoices in the period and groups items
/// by VAT rate (20%, 10%, 0%).
/// </summary>
public sealed record GetVatReturnQuery(int TenantId, int Month, int Year)
    : IRequest<Result<VatReturnDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetVatReturnQueryHandler
    : IRequestHandler<GetVatReturnQuery, Result<VatReturnDto>>
{
    private readonly IInvoiceRepository _invoices;

    public GetVatReturnQueryHandler(IInvoiceRepository invoices)
        => _invoices = invoices;

    public async Task<Result<VatReturnDto>> Handle(
        GetVatReturnQuery query,
        CancellationToken cancellationToken)
    {
        // Fetch all relevant invoices for the period (non-cancelled).
        var filter = new InvoiceFilter(
            Page:     1,
            PageSize: int.MaxValue,
            Status:   null,
            ClientId: null,
            Search:   null);

        var allInvoices = await _invoices.GetAllAsync(
            query.TenantId, filter, cancellationToken);

        // Filter to invoices issued during the requested month/year.
        var invoices = allInvoices
            .Where(inv =>
                inv.Status != InvoiceStatus.Otkazana &&
                inv.IssueDate.Month == query.Month &&
                inv.IssueDate.Year  == query.Year)
            .ToList();

        // Aggregate items by VAT rate.
        var allItems = invoices.SelectMany(inv => inv.Items).ToList();

        var items20 = allItems.Where(i => i.VatRate.Percent == 20m).ToList();
        var items10 = allItems.Where(i => i.VatRate.Percent == 10m).ToList();
        var items0  = allItems.Where(i => i.VatRate.Percent == 0m) .ToList();

        var taxableBase20   = Money.FromRSD(items20.Sum(i => i.Total.Amount));
        var vatCollected20  = Money.FromRSD(items20.Sum(i => i.VatAmount.Amount));
        var taxableBase10   = Money.FromRSD(items10.Sum(i => i.Total.Amount));
        var vatCollected10  = Money.FromRSD(items10.Sum(i => i.VatAmount.Amount));
        var taxableBase0    = Money.FromRSD(items0 .Sum(i => i.Total.Amount));

        // Input VAT (pretporez) — in a real system this would come from
        // purchase invoices or journal entries; here we use zero as a safe default.
        var inputVatDeductible = Money.Zero;

        var vatPayable = Money.FromRSD(
            vatCollected20.Amount + vatCollected10.Amount - inputVatDeductible.Amount);

        var dto = new VatReturnDto(
            Month:              query.Month,
            Year:               query.Year,
            TaxableBase20:      taxableBase20,
            VatCollected20:     vatCollected20,
            TaxableBase10:      taxableBase10,
            VatCollected10:     vatCollected10,
            TaxableBase0:       taxableBase0,
            InputVatDeductible: inputVatDeductible,
            VatPayable:         vatPayable);

        return Result<VatReturnDto>.Success(dto);
    }
}
