namespace AccountingERP.Application.Commands.Journal;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Aggregates.JournalEntry;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Domain.ValueObjects;
using FluentValidation;
using MediatR;

// ── Input DTOs ────────────────────────────────────────────────────────────────

/// <summary>Input DTO for a single line within a journal entry creation command.</summary>
public sealed record CreateJournalLineDto(
    int     AccountId,
    decimal Debit,
    decimal Credit,
    string? Note);

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Creates a new journal entry and immediately posts it (Proknjiženo).
/// The entry must be balanced (sum of debits == sum of credits) and the
/// accounting period must not be locked per Zakon o računovodstvu.
/// Returns the generated database ID on success.
/// </summary>
public sealed record CreateAndPostJournalEntryCommand(
    int                                TenantId,
    string                             Number,
    DateOnly                           Date,
    string                             Description,
    string                             UserId,
    IReadOnlyList<CreateJournalLineDto> Lines,
    string?                            SourceType = null,
    int?                               SourceId   = null) : IRequest<Result<int>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class CreateAndPostJournalEntryCommandValidator
    : AbstractValidator<CreateAndPostJournalEntryCommand>
{
    public CreateAndPostJournalEntryCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("TenantId mora biti pozitivan broj.");

        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Broj knjiženja je obavezan.")
            .MaximumLength(50).WithMessage("Broj knjiženja ne sme biti duži od 50 znakova.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Datum knjiženja je obavezan.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Opis knjiženja je obavezan.")
            .MaximumLength(500).WithMessage("Opis ne sme biti duži od 500 znakova.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId je obavezan.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Temeljnica mora imati najmanje jednu stavku.");

        // Balanced-entry rule: total debit must equal total credit.
        RuleFor(x => x.Lines)
            .Must(lines =>
            {
                if (lines is null || !lines.Any()) return true; // caught above
                var totalDebit  = lines.Sum(l => l.Debit);
                var totalCredit = lines.Sum(l => l.Credit);
                return Math.Abs(totalDebit - totalCredit) < 0.01m;
            })
            .WithMessage("Temeljnica mora biti uravnotežena: ukupno duguje mora biti jednako ukupno potražuje.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.AccountId)
                .GreaterThan(0).WithMessage("AccountId mora biti pozitivan broj.");

            line.RuleFor(l => l)
                .Must(l => !(l.Debit > 0 && l.Credit > 0))
                .WithMessage("Stavka ne može imati i duguje i potražuje istovremeno.");

            line.RuleFor(l => l)
                .Must(l => l.Debit >= 0 && l.Credit >= 0)
                .WithMessage("Iznosi duguje i potražuje moraju biti nenegativni.");

            line.RuleFor(l => l)
                .Must(l => l.Debit > 0 || l.Credit > 0)
                .WithMessage("Stavka mora imati ili duguje ili potražuje veće od nule.");
        });
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CreateAndPostJournalEntryCommandHandler
    : IRequestHandler<CreateAndPostJournalEntryCommand, Result<int>>
{
    private readonly IUnitOfWork  _uow;
    private readonly IHashService _hash;

    public CreateAndPostJournalEntryCommandHandler(IUnitOfWork uow, IHashService hash)
    {
        _uow  = uow;
        _hash = hash;
    }

    public async Task<Result<int>> Handle(
        CreateAndPostJournalEntryCommand command,
        CancellationToken                cancellationToken)
    {
        // Check period lock (Zakon o računovodstvu — retroaktivna knjiženja).
        var isLocked = await _uow.Journal.IsPeriodLockedAsync(
            command.TenantId, command.Date.Month, command.Date.Year, cancellationToken);

        if (isLocked)
            return Result<int>.Failure(
                $"Računovodstveni period {command.Date.Month:D2}/{command.Date.Year} je zaključan.");

        var tenantId = TenantId.From(command.TenantId);

        JournalEntry entry;
        try
        {
            entry = JournalEntry.Create(
                tenantId,
                command.Number,
                command.Date,
                command.Description,
                command.SourceType,
                command.SourceId);

            foreach (var line in command.Lines)
            {
                entry.AddLine(
                    line.AccountId,
                    Money.FromRSD(line.Debit),
                    Money.FromRSD(line.Credit),
                    line.Note);
            }
        }
        catch (DomainException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        // Build the integrity hash chain per Zakon o računovodstvu čl. 8.
        var previousHash = await _uow.Journal.GetLastHashAsync(command.TenantId, cancellationToken);
        var integrityHash = _hash.ComputeJournalHash(entry, previousHash);

        try
        {
            entry.Post(command.UserId, integrityHash, previousHash);
        }
        catch (DomainException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        _uow.Journal.Add(entry);
        await _uow.CommitAsync(cancellationToken);

        return Result<int>.Success(entry.Id);
    }
}
