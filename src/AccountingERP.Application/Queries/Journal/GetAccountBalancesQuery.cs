namespace AccountingERP.Application.Queries.Journal;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns the ledger account balances for the given tenant as of a specific date.
/// When <paramref name="AsOfDate"/> is null the current date is used.
/// Used for the trial balance and balance sheet reports.
/// </summary>
public sealed record GetAccountBalancesQuery(
    int       TenantId,
    DateOnly? AsOfDate = null) : IRequest<Result<IEnumerable<AccountBalance>>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetAccountBalancesQueryHandler
    : IRequestHandler<GetAccountBalancesQuery, Result<IEnumerable<AccountBalance>>>
{
    private readonly IUnitOfWork _uow;

    public GetAccountBalancesQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<AccountBalance>>> Handle(
        GetAccountBalancesQuery query,
        CancellationToken       cancellationToken)
    {
        var asOf     = query.AsOfDate ?? DateOnly.FromDateTime(DateTime.Today);
        var balances = await _uow.Journal.GetBalancesAsync(query.TenantId, asOf, cancellationToken);

        return Result<IEnumerable<AccountBalance>>.Success(balances);
    }
}
