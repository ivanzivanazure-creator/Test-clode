namespace AccountingERP.Application.Queries.Reports;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Domain.ValueObjects;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns a balance sheet as of the specified date.
/// Accounts are grouped by type: Assets (imovina), Liabilities (obaveze), Equity (kapital).
/// </summary>
public sealed record GetBalanceSheetQuery(int TenantId, DateOnly AsOfDate)
    : IRequest<Result<BalanceSheetDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetBalanceSheetQueryHandler
    : IRequestHandler<GetBalanceSheetQuery, Result<BalanceSheetDto>>
{
    private readonly IJournalRepository _journal;

    public GetBalanceSheetQueryHandler(IJournalRepository journal)
        => _journal = journal;

    public async Task<Result<BalanceSheetDto>> Handle(
        GetBalanceSheetQuery query,
        CancellationToken    cancellationToken)
    {
        var balances = await _journal.GetBalancesAsync(
            query.TenantId, query.AsOfDate, cancellationToken);

        // Serbian chart of accounts:
        // Assets      — klasa 0 (dugotrajna imovina) + klasa 1 (zalihe) + klasa 2 (potraživanja/gotovina)
        // Liabilities — klasa 4 (obaveze)
        // Equity      — klasa 3 (kapital)
        var assets      = balances.Where(b => b.Type == "Asset")
                                  .Select(b => new ReportLineDto(b.Code, b.Name, b.Balance))
                                  .OrderBy(l => l.Code)
                                  .ToList();

        var liabilities = balances.Where(b => b.Type == "Liability")
                                  .Select(b => new ReportLineDto(b.Code, b.Name, b.Balance))
                                  .OrderBy(l => l.Code)
                                  .ToList();

        var equity      = balances.Where(b => b.Type == "Equity")
                                  .Select(b => new ReportLineDto(b.Code, b.Name, b.Balance))
                                  .OrderBy(l => l.Code)
                                  .ToList();

        var totalAssets      = Money.FromRSD(assets     .Sum(l => l.Balance.Amount));
        var totalLiabilities = Money.FromRSD(liabilities.Sum(l => l.Balance.Amount));
        var totalEquity      = Money.FromRSD(equity     .Sum(l => l.Balance.Amount));

        var dto = new BalanceSheetDto(
            AsOf:             query.AsOfDate,
            Assets:           assets,
            Liabilities:      liabilities,
            Equity:           equity,
            TotalAssets:      totalAssets,
            TotalLiabilities: totalLiabilities,
            TotalEquity:      totalEquity);

        return Result<BalanceSheetDto>.Success(dto);
    }
}
