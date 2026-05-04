namespace AccountingERP.Application.Queries.Invoices;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns aggregated invoice KPIs for the given tenant:
/// total count, total issued amount, paid amount and overdue amount.
/// Used by the dashboard and reporting screens.
/// </summary>
public sealed record GetInvoiceSummaryQuery(int TenantId) : IRequest<Result<InvoiceSummaryData>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetInvoiceSummaryQueryHandler
    : IRequestHandler<GetInvoiceSummaryQuery, Result<InvoiceSummaryData>>
{
    private readonly IUnitOfWork _uow;

    public GetInvoiceSummaryQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<InvoiceSummaryData>> Handle(
        GetInvoiceSummaryQuery query,
        CancellationToken      cancellationToken)
    {
        var summary = await _uow.Invoices.GetSummaryAsync(query.TenantId, cancellationToken);
        return Result<InvoiceSummaryData>.Success(summary);
    }
}
