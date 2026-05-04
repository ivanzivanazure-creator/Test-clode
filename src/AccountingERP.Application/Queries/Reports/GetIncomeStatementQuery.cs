namespace AccountingERP.Application.Queries.Reports;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Domain.ValueObjects;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns an income statement (bilans uspjeha) for the given month and year.
/// Revenue accounts are in class 6xx, expense accounts in class 5xx.
/// </summary>
public sealed record GetIncomeStatementQuery(int TenantId, int Month, int Year)
    : IRequest<Result<IncomeStatementDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetIncomeStatementQueryHandler
    : IRequestHandler<GetIncomeStatementQuery, Result<IncomeStatementDto>>
{
    private readonly IJournalRepository _journal;

    public GetIncomeStatementQueryHandler(IJournalRepository journal)
        => _journal = journal;

    public async Task<Result<IncomeStatementDto>> Handle(
        GetIncomeStatementQuery query,
        CancellationToken       cancellationToken)
    {
        // Get all balances as of the last day of the requested month.
        var lastDay = new DateOnly(query.Year, query.Month,
            DateTime.DaysInMonth(query.Year, query.Month));

        var balances = await _journal.GetBalancesAsync(
            query.TenantId, lastDay, cancellationToken);

        // Revenue — klasa 6 (prihodi)
        var revenue = balances
            .Where(b => b.Code.StartsWith("6"))
            .Select(b => new ReportLineDto(b.Code, b.Name, b.Balance))
            .OrderBy(l => l.Code)
            .ToList();

        // Expenses — klasa 5 (rashodi)
        var expenses = balances
            .Where(b => b.Code.StartsWith("5"))
            .Select(b => new ReportLineDto(b.Code, b.Name, b.Balance))
            .OrderBy(l => l.Code)
            .ToList();

        var totalRevenue  = Money.FromRSD(revenue .Sum(l => l.Balance.Amount));
        var totalExpenses = Money.FromRSD(expenses.Sum(l => l.Balance.Amount));
        var netIncome     = Money.FromRSD(totalRevenue.Amount - totalExpenses.Amount);

        var period = new AccountingPeriod(query.Month, query.Year);

        var dto = new IncomeStatementDto(
            Period:        period,
            Revenue:       revenue,
            Expenses:      expenses,
            TotalRevenue:  totalRevenue,
            TotalExpenses: totalExpenses,
            NetIncome:     netIncome);

        return Result<IncomeStatementDto>.Success(dto);
    }
}
