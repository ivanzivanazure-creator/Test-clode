namespace AccountingERP.Application.Commands.Clients;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record UpdateClientCommand(
    int     Id,
    string  Name,
    string? PIB         = null,
    string? MaticniBroj = null,
    string? Address     = null,
    string? City        = null,
    string? IBAN        = null,
    string? Email       = null,
    string? Phone       = null) : IRequest<Result<Unit>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, Result<Unit>>
{
    private readonly IUnitOfWork _uow;

    public UpdateClientCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Unit>> Handle(
        UpdateClientCommand command,
        CancellationToken   cancellationToken)
    {
        var client = await _uow.Clients.GetByIdAsync(command.Id, cancellationToken);
        if (client is null)
            return Result<Unit>.Failure($"Klijent sa ID={command.Id} nije pronađen.");

        try
        {
            client.Update(command.Name, command.PIB, command.MaticniBroj,
                          command.Address, command.City,
                          command.IBAN, command.Email, command.Phone);
        }
        catch (DomainException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }

        await _uow.CommitAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
