namespace AccountingERP.Application.Commands.Clients;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Aggregates.Client;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using FluentValidation;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record CreateClientCommand(
    int        TenantId,
    string     Name,
    ClientType ClientType,
    string?    PIB         = null,
    string?    MaticniBroj = null,
    string?    Address     = null,
    string?    City        = null,
    string?    IBAN        = null,
    string?    Email       = null,
    string?    Phone       = null) : IRequest<Result<int>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("TenantId mora biti pozitivan broj.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv klijenta je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne sme biti duži od 200 znakova.");

        RuleFor(x => x.PIB)
            .Matches(@"^\d{9}$").WithMessage("PIB mora sadržati tačno 9 cifara.")
            .When(x => !string.IsNullOrEmpty(x.PIB));

        RuleFor(x => x.MaticniBroj)
            .Matches(@"^\d{8}$").WithMessage("Matični broj mora sadržati tačno 8 cifara.")
            .When(x => !string.IsNullOrEmpty(x.MaticniBroj));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email adresa nije ispravna.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<int>>
{
    private readonly IUnitOfWork _uow;

    public CreateClientCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> Handle(
        CreateClientCommand command,
        CancellationToken   cancellationToken)
    {
        // PIB uniqueness check per tenant
        if (!string.IsNullOrEmpty(command.PIB))
        {
            var pibExists = await _uow.Clients.PIBExistsAsync(
                command.TenantId, command.PIB, cancellationToken);
            if (pibExists)
                return Result<int>.Failure($"Klijent sa PIB-om '{command.PIB}' već postoji.");
        }

        Client client;
        try
        {
            client = Client.Create(command.TenantId, command.Name, command.ClientType);
        }
        catch (DomainException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        if (command.PIB is not null || command.MaticniBroj is not null
            || command.Address is not null || command.City is not null
            || command.IBAN is not null || command.Email is not null
            || command.Phone is not null)
        {
            client.Update(command.Name, command.PIB, command.MaticniBroj,
                          command.Address, command.City,
                          command.IBAN, command.Email, command.Phone);
        }

        _uow.Clients.Add(client);
        await _uow.CommitAsync(cancellationToken);

        return Result<int>.Success(client.Id);
    }
}
