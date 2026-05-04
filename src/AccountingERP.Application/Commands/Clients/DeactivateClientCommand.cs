namespace AccountingERP.Application.Commands.Clients;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record DeactivateClientCommand(int Id) : IRequest<Result<Unit>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class DeactivateClientCommandHandler : IRequestHandler<DeactivateClientCommand, Result<Unit>>
{
    private readonly IUnitOfWork _uow;

    public DeactivateClientCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Unit>> Handle(
        DeactivateClientCommand command,
        CancellationToken       cancellationToken)
    {
        var client = await _uow.Clients.GetByIdAsync(command.Id, cancellationToken);
        if (client is null)
            return Result<Unit>.Failure($"Klijent sa ID={command.Id} nije pronađen.");

        client.Deactivate();
        await _uow.CommitAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
