namespace AccountingERP.Application.Queries.Employees;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Computes and returns the full payroll breakdown for an active employee.
/// The calculation uses the 2024 Serbian tax and contribution rates as defined
/// in the domain model (<c>Employee.CalculateNetSalary()</c>).
/// </summary>
public sealed record GetPayrollCalculationQuery(
    int EmployeeId) : IRequest<Result<PayrollResultDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetPayrollCalculationQueryHandler
    : IRequestHandler<GetPayrollCalculationQuery, Result<PayrollResultDto>>
{
    private readonly IUnitOfWork _uow;

    public GetPayrollCalculationQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PayrollResultDto>> Handle(
        GetPayrollCalculationQuery query,
        CancellationToken          cancellationToken)
    {
        var employee = await _uow.Employees.GetByIdAsync(query.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<PayrollResultDto>.Failure(
                $"Zaposleni sa ID={query.EmployeeId} nije pronađen.");

        if (!employee.IsActive)
            return Result<PayrollResultDto>.Failure(
                "Nije moguće izračunati platu za neaktivnog zaposlenog.");

        var calculation = employee.CalculateNetSalary();
        var dto         = PayrollResultDto.FromDomain(calculation);

        return Result<PayrollResultDto>.Success(dto);
    }
}
