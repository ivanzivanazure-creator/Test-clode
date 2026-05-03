namespace AccountingERP.Application.Commands.Invoices;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using FluentValidation;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Transitions an invoice from Nacrt (draft) to Izdata (issued) status.
/// The issuing user ID is recorded on the domain event for audit purposes.
/// </summary>
public sealed record IssueInvoiceCommand(
    int    InvoiceId,
    string UserId) : IRequest<Result<Unit>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage("InvoiceId mora biti pozitivan broj.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId je obavezan.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class IssueInvoiceCommandHandler : IRequestHandler<IssueInvoiceCommand, Result<Unit>>
{
    private readonly IUnitOfWork _uow;

    public IssueInvoiceCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Unit>> Handle(
        IssueInvoiceCommand command,
        CancellationToken   cancellationToken)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result<Unit>.Failure($"Faktura sa ID={command.InvoiceId} nije pronađena.");

        try
        {
            invoice.Issue(command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }

        await _uow.CommitAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
