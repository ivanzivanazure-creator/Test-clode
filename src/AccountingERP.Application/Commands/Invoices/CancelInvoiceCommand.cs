namespace AccountingERP.Application.Commands.Invoices;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using FluentValidation;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Cancels an invoice (Otkazana status).
/// Paid invoices cannot be cancelled — a credit note must be issued instead.
/// </summary>
public sealed record CancelInvoiceCommand(
    int InvoiceId) : IRequest<Result<Unit>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class CancelInvoiceCommandValidator : AbstractValidator<CancelInvoiceCommand>
{
    public CancelInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage("InvoiceId mora biti pozitivan broj.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, Result<Unit>>
{
    private readonly IUnitOfWork _uow;

    public CancelInvoiceCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Unit>> Handle(
        CancelInvoiceCommand command,
        CancellationToken    cancellationToken)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result<Unit>.Failure($"Faktura sa ID={command.InvoiceId} nije pronađena.");

        try
        {
            invoice.Cancel();
        }
        catch (DomainException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }

        await _uow.CommitAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
