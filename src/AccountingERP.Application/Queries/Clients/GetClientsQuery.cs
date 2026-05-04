namespace AccountingERP.Application.Queries.Clients;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetClientsQuery(
    int     TenantId,
    string? Search = null) : IRequest<Result<IEnumerable<ClientDto>>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetClientsQueryHandler
    : IRequestHandler<GetClientsQuery, Result<IEnumerable<ClientDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetClientsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<ClientDto>>> Handle(
        GetClientsQuery   query,
        CancellationToken cancellationToken)
    {
        var clients = await _uow.Clients.GetAllAsync(
            query.TenantId, query.Search, cancellationToken);

        var dtos = clients.Select(ClientDto.FromDomain);
        return Result<IEnumerable<ClientDto>>.Success(dtos);
    }
}
