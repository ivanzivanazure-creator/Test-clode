namespace AccountingERP.Application.Commands.Invoices;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using FluentValidation;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Marks an invoice as paid (Plaćena). The payment date must not be in the future.
/// Both Izdata (issued) and Dospela (overdue) invoices are eligible for payment.
/// </summary>
public sealed record MarkInvoicePaidCommand(
    int      InvoiceId,
    DateOnly PaymentDate) : IRequest<Result<Unit>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class MarkInvoicePaidCommandValidator : AbstractValidator<MarkInvoicePaidCommand>
{
    public MarkInvoicePaidCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage("InvoiceId mora biti pozitivan broj.");

        RuleFor(x => x.PaymentDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Datum naplate ne može biti u budućnosti.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class MarkInvoicePaidCommandHandler : IRequestHandler<MarkInvoicePaidCommand, Result<Unit>>
{
    private readonly IUnitOfWork _uow;

    public MarkInvoicePaidCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Unit>> Handle(
        MarkInvoicePaidCommand command,
        CancellationToken      cancellationToken)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result<Unit>.Failure($"Faktura sa ID={command.InvoiceId} nije pronađena.");

        try
        {
            invoice.MarkPaid(command.PaymentDate);
        }
        catch (DomainException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }

        await _uow.CommitAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
