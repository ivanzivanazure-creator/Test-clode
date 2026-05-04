namespace AccountingERP.Application.Queries.Employees;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns all active employees for the given tenant.
/// Sensitive encrypted fields (JMBG, bank account, e-mail) are never included
/// in the DTO — they are only accessible through dedicated, audited operations.
/// </summary>
public sealed record GetEmployeesQuery(int TenantId) : IRequest<Result<IEnumerable<EmployeeDto>>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetEmployeesQueryHandler
    : IRequestHandler<GetEmployeesQuery, Result<IEnumerable<EmployeeDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetEmployeesQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<EmployeeDto>>> Handle(
        GetEmployeesQuery query,
        CancellationToken cancellationToken)
    {
        var employees = await _uow.Employees.GetActiveAsync(query.TenantId, cancellationToken);
        var dtos      = employees.Select(EmployeeDto.FromDomain);

        return Result<IEnumerable<EmployeeDto>>.Success(dtos);
    }
}
