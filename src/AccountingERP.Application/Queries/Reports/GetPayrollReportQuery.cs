namespace AccountingERP.Application.Queries.Reports;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Domain.ValueObjects;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns a payroll report (obračun zarada) for all active employees,
/// calculated using the Serbian 2024 tax/contribution rates in the domain model.
/// </summary>
public sealed record GetPayrollReportQuery(int TenantId, int Month, int Year)
    : IRequest<Result<PayrollReportDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetPayrollReportQueryHandler
    : IRequestHandler<GetPayrollReportQuery, Result<PayrollReportDto>>
{
    private readonly IEmployeeRepository _employees;

    public GetPayrollReportQueryHandler(IEmployeeRepository employees)
        => _employees = employees;

    public async Task<Result<PayrollReportDto>> Handle(
        GetPayrollReportQuery query,
        CancellationToken     cancellationToken)
    {
        var employees = await _employees.GetActiveAsync(query.TenantId, cancellationToken);

        var lines = employees
            .Select(emp =>
            {
                var calc = emp.CalculateNetSalary();
                return new EmployeePayrollLine(
                    FullName:  emp.FullName,
                    Position:  emp.Position,
                    Gross:     calc.GrossBase,
                    Net:       calc.NetSalary,
                    Tax:       calc.IncomeTax,
                    TotalCost: calc.TotalCost);
            })
            .OrderBy(l => l.FullName)
            .ToList();

        var totalGross                = Money.FromRSD(lines.Sum(l => l.Gross.Amount));
        var totalNet                  = Money.FromRSD(lines.Sum(l => l.Net.Amount));
        var totalTax                  = Money.FromRSD(lines.Sum(l => l.Tax.Amount));
        var totalEmployerContributions = Money.FromRSD(
            lines.Sum(l => l.TotalCost.Amount - l.Gross.Amount));

        var dto = new PayrollReportDto(
            Month:                      query.Month,
            Year:                       query.Year,
            Lines:                      lines,
            TotalGross:                 totalGross,
            TotalNet:                   totalNet,
            TotalTax:                   totalTax,
            TotalEmployerContributions: totalEmployerContributions);

        return Result<PayrollReportDto>.Success(dto);
    }
}
