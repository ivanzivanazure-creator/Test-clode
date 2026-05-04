namespace AccountingERP.Application.Queries.Dashboard;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Aggregates.Invoice;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Aggregates all KPIs required for the main accounting dashboard:
/// invoice counts and revenue totals, active employee count, total payroll cost,
/// and a 12-month rolling revenue breakdown for chart rendering.
/// </summary>
public sealed record GetDashboardQuery(int TenantId) : IRequest<Result<DashboardDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetDashboardQueryHandler
    : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private readonly IUnitOfWork _uow;

    public GetDashboardQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<DashboardDto>> Handle(
        GetDashboardQuery query,
        CancellationToken cancellationToken)
    {
        // ── Invoice summary ────────────────────────────────────────────────────
        var summary = await _uow.Invoices.GetSummaryAsync(query.TenantId, cancellationToken);

        // Overdue count: fetch overdue invoices from last 12 months.
        var overdueFilter = new InvoiceFilter(
            Page:     1,
            PageSize: int.MaxValue,
            Status:   "Dospela",
            ClientId: null,
            Search:   null);

        var overdueInvoices = (await _uow.Invoices
            .GetAllAsync(query.TenantId, overdueFilter, cancellationToken))
            .ToList();

        int overdueCount = overdueInvoices.Count;

        // ── Employees ─────────────────────────────────────────────────────────
        var activeEmployees = (await _uow.Employees
            .GetActiveAsync(query.TenantId, cancellationToken))
            .ToList();

        int     activeCount      = activeEmployees.Count;
        decimal totalPayrollCost = activeEmployees
            .Sum(e => e.CalculateNetSalary().TotalCost.Amount);

        // ── Monthly revenue (rolling 12 months) ───────────────────────────────
        var monthlyRevenue = await BuildMonthlyRevenueAsync(query.TenantId, cancellationToken);

        var dto = new DashboardDto(
            TotalInvoices:    summary.TotalCount,
            TotalRevenue:     summary.TotalAmount.Amount,
            PaidRevenue:      summary.PaidAmount.Amount,
            OverdueRevenue:   summary.OverdueAmount.Amount,
            OverdueCount:     overdueCount,
            ActiveEmployees:  activeCount,
            TotalPayrollCost: totalPayrollCost,
            MonthlyRevenue:   monthlyRevenue);

        return Result<DashboardDto>.Success(dto);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<IReadOnlyList<MonthlyRevenueDto>> BuildMonthlyRevenueAsync(
        int               tenantId,
        CancellationToken ct)
    {
        var today    = DateTime.Today;
        var result   = new List<MonthlyRevenueDto>(12);

        for (int i = 11; i >= 0; i--)
        {
            var monthDate  = today.AddMonths(-i);
            var year       = monthDate.Year;
            var month      = monthDate.Month;
            var firstDay   = new DateOnly(year, month, 1);
            var lastDay    = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

            // Fetch all invoices issued in this calendar month.
            // We use Status=null to get both Izdata and Plaćena invoices.
            var monthFilter = new InvoiceFilter(
                Page:     1,
                PageSize: int.MaxValue,
                Status:   null,
                ClientId: null,
                Search:   null);

            var monthInvoices = (await _uow.Invoices
                .GetAllAsync(tenantId, monthFilter, ct))
                .Where(inv => inv.IssueDate >= firstDay && inv.IssueDate <= lastDay)
                .ToList();

            decimal issued = monthInvoices
                .Where(inv => inv.Status != InvoiceStatus.Otkazana)
                .Sum(inv => inv.TotalAmount.Amount);

            decimal paid = monthInvoices
                .Where(inv => inv.Status == InvoiceStatus.Plaćena)
                .Sum(inv => inv.TotalAmount.Amount);

            result.Add(new MonthlyRevenueDto(
                Year:         year,
                Month:        month,
                MonthName:    monthDate.ToString("MMMM yyyy"),
                IssuedAmount: issued,
                PaidAmount:   paid));
        }

        return result.AsReadOnly();
    }
}
