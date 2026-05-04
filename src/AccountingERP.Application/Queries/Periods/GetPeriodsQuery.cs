namespace AccountingERP.Application.Queries.Periods;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetPeriodsQuery(
    int TenantId,
    int Year) : IRequest<Result<IEnumerable<AccountingPeriodDto>>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetPeriodsQueryHandler
    : IRequestHandler<GetPeriodsQuery, Result<IEnumerable<AccountingPeriodDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetPeriodsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<AccountingPeriodDto>>> Handle(
        GetPeriodsQuery   query,
        CancellationToken cancellationToken)
    {
        var periods = await _uow.Periods.GetByYearAsync(
            query.TenantId, query.Year, cancellationToken);

        var dtos = periods.Select(AccountingPeriodDto.FromDomain);
        return Result<IEnumerable<AccountingPeriodDto>>.Success(dtos);
    }
}
