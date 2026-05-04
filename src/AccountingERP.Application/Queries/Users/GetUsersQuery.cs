namespace AccountingERP.Application.Queries.Users;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetUsersQuery(int TenantId) : IRequest<Result<IEnumerable<UserDto>>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, Result<IEnumerable<UserDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetUsersQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<UserDto>>> Handle(
        GetUsersQuery     query,
        CancellationToken cancellationToken)
    {
        var users = await _uow.Users.GetAllAsync(query.TenantId, cancellationToken);
        var dtos  = users.Select(UserDto.FromDomain);
        return Result<IEnumerable<UserDto>>.Success(dtos);
    }
}
