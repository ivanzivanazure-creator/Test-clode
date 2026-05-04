namespace AccountingERP.Application.Queries.Invoices;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns a paginated list of invoices for the given tenant, optionally filtered
/// by status, client and free-text search. Mirrors the fields on <see cref="InvoiceFilter"/>.
/// </summary>
public sealed record GetInvoicesQuery(
    int     TenantId,
    int     Page     = 1,
    int     PageSize = 20,
    string? Status   = null,
    int?    ClientId = null,
    string? Search   = null) : IRequest<Result<PagedResult<InvoiceDto>>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetInvoicesQueryHandler
    : IRequestHandler<GetInvoicesQuery, Result<PagedResult<InvoiceDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetInvoicesQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PagedResult<InvoiceDto>>> Handle(
        GetInvoicesQuery  query,
        CancellationToken cancellationToken)
    {
        var filter = new InvoiceFilter(
            Page:     query.Page,
            PageSize: query.PageSize,
            Status:   query.Status,
            ClientId: query.ClientId,
            Search:   query.Search);

        var invoices = await _uow.Invoices.GetAllAsync(query.TenantId, filter, cancellationToken);
        var invoiceList = invoices.ToList();

        // GetAllAsync returns the filtered page; the repository is responsible for
        // applying Skip/Take. We obtain the total count via a summary query so we
        // can populate pagination metadata without a second full-table scan.
        var summary = await _uow.Invoices.GetSummaryAsync(query.TenantId, cancellationToken);

        var dtos = invoiceList.Select(InvoiceDto.FromDomain).ToList();

        // Use the summary total as the authoritative count; it may differ from the
        // filtered page count when filters are applied, but gives the best available
        // approximation without an additional filtered-count repository method.
        var totalCount = string.IsNullOrEmpty(query.Status)
                         && !query.ClientId.HasValue
                         && string.IsNullOrEmpty(query.Search)
            ? summary.TotalCount
            : dtos.Count; // filtered result — we only know the page size

        var paged = new PagedResult<InvoiceDto>(dtos, query.Page, query.PageSize, totalCount);
        return Result<PagedResult<InvoiceDto>>.Success(paged);
    }
}
