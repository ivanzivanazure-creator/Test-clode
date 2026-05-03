namespace AccountingERP.Application.Commands.Invoices;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Aggregates.Invoice;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Domain.ValueObjects;
using FluentValidation;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Creates a new invoice in Nacrt (draft) status.
/// Returns the generated database ID on success.
/// </summary>
public sealed record CreateInvoiceCommand(
    int                            TenantId,
    string                         Number,
    int                            ClientId,
    DateOnly                       DueDate,
    IReadOnlyList<CreateInvoiceItemDto> Items) : IRequest<Result<int>>;

/// <summary>Input DTO for a single invoice line item within the create command.</summary>
public sealed record CreateInvoiceItemDto(
    string  Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatPercent,
    string  Unit = "kom");

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("TenantId mora biti pozitivan broj.");

        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Broj fakture je obavezan.")
            .MaximumLength(50).WithMessage("Broj fakture ne sme biti duži od 50 znakova.");

        RuleFor(x => x.ClientId)
            .GreaterThan(0).WithMessage("ClientId mora biti pozitivan broj.");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Datum dospeća ne sme biti u prošlosti.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Faktura mora sadržati najmanje jednu stavku.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description)
                .NotEmpty().WithMessage("Opis stavke je obavezan.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Količina mora biti veća od nule.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0).WithMessage("Jedinična cena mora biti veća od nule.");

            item.RuleFor(i => i.VatPercent)
                .Must(v => v == 0m || v == 8m || v == 10m || v == 20m)
                .WithMessage("PDV stopa mora biti 0, 8, 10 ili 20%.");
        });
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<int>>
{
    private readonly IUnitOfWork     _uow;
    private readonly IHashService    _hash;

    public CreateInvoiceCommandHandler(IUnitOfWork uow, IHashService hash)
    {
        _uow  = uow;
        _hash = hash;
    }

    public async Task<Result<int>> Handle(
        CreateInvoiceCommand command,
        CancellationToken    cancellationToken)
    {
        // Uniqueness check — number must be unique per tenant.
        var exists = await _uow.Invoices.NumberExistsAsync(
            command.TenantId, command.Number, cancellationToken);

        if (exists)
            return Result<int>.Failure(
                $"Faktura sa brojem '{command.Number}' već postoji.");

        var tenantId = TenantId.From(command.TenantId);

        var items = command.Items.Select(i => (
            description: i.Description,
            qty:         i.Quantity,
            unitPrice:   Money.FromRSD(i.UnitPrice),
            vatRate:     new VatRate(i.VatPercent)));

        Invoice invoice;
        try
        {
            invoice = Invoice.Create(tenantId, command.Number, command.ClientId,
                                     command.DueDate, items);
        }
        catch (DomainException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        // Compute integrity hash per Zakon o računovodstvu čl. 8.
        var integrityHash = _hash.ComputeInvoiceHash(invoice);
        invoice.SetIntegrityHash(integrityHash);

        _uow.Invoices.Add(invoice);
        await _uow.CommitAsync(cancellationToken);

        return Result<int>.Success(invoice.Id);
    }
}
