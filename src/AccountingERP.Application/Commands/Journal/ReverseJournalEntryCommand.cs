namespace AccountingERP.Application.Commands.Journal;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Aggregates.JournalEntry;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using FluentValidation;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Creates a reversal (storno) of an existing posted journal entry.
/// The original entry is moved to Stornirano status and a new mirror entry
/// is created, posted, and assigned the given number.
/// Returns the ID of the newly created reversal entry.
/// </summary>
public sealed record ReverseJournalEntryCommand(
    int    EntryId,
    string NewNumber,
    string UserId) : IRequest<Result<int>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class ReverseJournalEntryCommandValidator : AbstractValidator<ReverseJournalEntryCommand>
{
    public ReverseJournalEntryCommandValidator()
    {
        RuleFor(x => x.EntryId)
            .GreaterThan(0).WithMessage("EntryId mora biti pozitivan broj.");

        RuleFor(x => x.NewNumber)
            .NotEmpty().WithMessage("Broj storno knjiženja je obavezan.")
            .MaximumLength(50).WithMessage("Broj knjiženja ne sme biti duži od 50 znakova.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId je obavezan.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class ReverseJournalEntryCommandHandler
    : IRequestHandler<ReverseJournalEntryCommand, Result<int>>
{
    private readonly IUnitOfWork  _uow;
    private readonly IHashService _hash;

    public ReverseJournalEntryCommandHandler(IUnitOfWork uow, IHashService hash)
    {
        _uow  = uow;
        _hash = hash;
    }

    public async Task<Result<int>> Handle(
        ReverseJournalEntryCommand command,
        CancellationToken          cancellationToken)
    {
        var original = await _uow.Journal.GetByIdAsync(command.EntryId, cancellationToken);
        if (original is null)
            return Result<int>.Failure($"Temeljnica sa ID={command.EntryId} nije pronađena.");

        // Verify the reversal period is not locked.
        var today    = DateOnly.FromDateTime(DateTime.Today);
        var isLocked = await _uow.Journal.IsPeriodLockedAsync(
            original.TenantId.Value, today.Month, today.Year, cancellationToken);

        if (isLocked)
            return Result<int>.Failure(
                $"Računovodstveni period {today.Month:D2}/{today.Year} je zaključan — storno nije moguć.");

        Domain.Aggregates.JournalEntry.JournalEntry reversal;
        try
        {
            reversal = original.CreateReversal(command.NewNumber);
        }
        catch (DomainException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        // Build the integrity hash chain for the new reversal entry.
        var previousHash  = await _uow.Journal.GetLastHashAsync(original.TenantId.Value, cancellationToken);
        var integrityHash = _hash.ComputeJournalHash(reversal, previousHash);

        try
        {
            reversal.Post(command.UserId, integrityHash, previousHash);
        }
        catch (DomainException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        _uow.Journal.Add(reversal);
        await _uow.CommitAsync(cancellationToken);

        return Result<int>.Success(reversal.Id);
    }
}
