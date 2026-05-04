namespace AccountingERP.Application.Queries.Journal;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Aggregates.JournalEntry;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns a paginated list of journal entries (temeljnice) for the given tenant.
/// Optional filters narrow results by status, date range, and source type.
/// </summary>
public sealed record GetJournalEntriesQuery(
    int       TenantId,
    int       Page       = 1,
    int       PageSize   = 20,
    string?   Status     = null,
    DateOnly? DateFrom   = null,
    DateOnly? DateTo     = null,
    string?   SourceType = null) : IRequest<Result<PagedResult<JournalEntryDto>>>;

// ── Handler ───────────────────────────────────────────────────────────────────

/// <summary>
/// Handles <see cref="GetJournalEntriesQuery"/>.
/// The concrete <see cref="IJournalRepository"/> implementation (in the
/// Infrastructure layer) is expected to implement <c>GetAllAsync</c> that accepts
/// a filter and returns a pre-paged collection. This handler references that via
/// the extended interface <see cref="IJournalEntriesReader"/> which the
/// Infrastructure project registers in the DI container.
/// </summary>
public sealed class GetJournalEntriesQueryHandler
    : IRequestHandler<GetJournalEntriesQuery, Result<PagedResult<JournalEntryDto>>>
{
    private readonly IUnitOfWork            _uow;
    private readonly IJournalEntriesReader  _reader;

    public GetJournalEntriesQueryHandler(IUnitOfWork uow, IJournalEntriesReader reader)
    {
        _uow    = uow;
        _reader = reader;
    }

    public async Task<Result<PagedResult<JournalEntryDto>>> Handle(
        GetJournalEntriesQuery query,
        CancellationToken      cancellationToken)
    {
        var filter = new JournalFilter(
            Page:       query.Page,
            PageSize:   query.PageSize,
            Status:     query.Status,
            DateFrom:   query.DateFrom,
            DateTo:     query.DateTo,
            SourceType: query.SourceType);

        var (entries, totalCount) = await _reader.GetPagedAsync(
            query.TenantId, filter, cancellationToken);

        var dtos  = entries.Select(JournalEntryDto.FromDomain).ToList();
        var paged = new PagedResult<JournalEntryDto>(dtos, query.Page, query.PageSize, totalCount);

        return Result<PagedResult<JournalEntryDto>>.Success(paged);
    }
}

// ── Supporting types ──────────────────────────────────────────────────────────

/// <summary>
/// Filter record for paginated journal entry queries.
/// Mirrors <see cref="InvoiceFilter"/> in pattern.
/// </summary>
public sealed record JournalFilter(
    int       Page,
    int       PageSize,
    string?   Status,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string?   SourceType);

/// <summary>
/// Read-side interface for fetching paginated journal entries.
/// Implemented in the Infrastructure layer alongside <see cref="IJournalRepository"/>.
/// Kept separate to honour the CQRS separation without adding write-side concerns
/// to the query path.
/// </summary>
public interface IJournalEntriesReader
{
    Task<(IEnumerable<JournalEntry> Entries, int TotalCount)> GetPagedAsync(
        int               tenantId,
        JournalFilter     filter,
        CancellationToken ct = default);
}
