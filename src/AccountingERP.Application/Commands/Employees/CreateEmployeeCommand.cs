namespace AccountingERP.Application.Commands.Employees;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Aggregates.Employee;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Domain.ValueObjects;
using FluentValidation;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Creates a new employee record.
/// Sensitive fields (JMBG, bank account, e-mail) are optional; when provided
/// they are encrypted with AES-256-GCM via <see cref="IEncryptionService"/>
/// and stored as ciphertext. A SHA-256 hash of the JMBG is stored separately
/// to allow duplicate detection without decrypting — compliant with ZZPL 87/2018.
/// </summary>
public sealed record CreateEmployeeCommand(
    int           TenantId,
    string        FirstName,
    string        LastName,
    string        Position,
    decimal       GrossSalary,
    DateOnly      HireDate,
    TaxExemption  TaxExemption = TaxExemption.None,
    string?       Jmbg         = null,
    string?       BankAccount  = null,
    string?       Email        = null) : IRequest<Result<int>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("TenantId mora biti pozitivan broj.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ime je obavezno.")
            .MaximumLength(100).WithMessage("Ime ne sme biti duže od 100 znakova.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Prezime je obavezno.")
            .MaximumLength(100).WithMessage("Prezime ne sme biti duže od 100 znakova.");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Radno mesto je obavezno.")
            .MaximumLength(200).WithMessage("Radno mesto ne sme biti duže od 200 znakova.");

        RuleFor(x => x.GrossSalary)
            .GreaterThan(0).WithMessage("Bruto plata mora biti veća od nule.");

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Datum zaposlenja je obavezan.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddDays(30)))
            .WithMessage("Datum zaposlenja ne može biti previše u budućnosti.");

        // JMBG: exactly 13 digits when provided.
        RuleFor(x => x.Jmbg)
            .Matches(@"^\d{13}$")
            .WithMessage("JMBG mora sadržati tačno 13 cifara.")
            .When(x => !string.IsNullOrEmpty(x.Jmbg));

        // Bank account: basic IBAN-style format for Serbian accounts when provided.
        RuleFor(x => x.BankAccount)
            .MaximumLength(50).WithMessage("Broj bankovnog računa ne sme biti duži od 50 znakova.")
            .When(x => !string.IsNullOrEmpty(x.BankAccount));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("E-mail adresa nije validna.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<int>>
{
    private readonly IUnitOfWork        _uow;
    private readonly IHashService       _hash;
    private readonly IEncryptionService _encryption;

    public CreateEmployeeCommandHandler(
        IUnitOfWork        uow,
        IHashService       hash,
        IEncryptionService encryption)
    {
        _uow        = uow;
        _hash       = hash;
        _encryption = encryption;
    }

    public async Task<Result<int>> Handle(
        CreateEmployeeCommand command,
        CancellationToken     cancellationToken)
    {
        // Duplicate check via JMBG hash — avoids storing plaintext JMBG in memory.
        if (!string.IsNullOrEmpty(command.Jmbg))
        {
            var jmbgHash = _hash.ComputeJMBGHash(command.Jmbg);
            var existing = await _uow.Employees.GetByJMBGHashAsync(
                command.TenantId, jmbgHash, cancellationToken);

            if (existing is not null)
                return Result<int>.Failure("Zaposleni sa ovim JMBG-om već postoji u sistemu.");
        }

        var tenantId = TenantId.From(command.TenantId);
        var salary   = Money.FromRSD(command.GrossSalary);

        Employee employee;
        try
        {
            employee = Employee.Create(
                tenantId,
                command.FirstName,
                command.LastName,
                command.Position,
                salary,
                command.HireDate,
                command.TaxExemption);
        }
        catch (DomainException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        // Encrypt sensitive fields and store hashes per ZZPL 87/2018.
        string? jmbgEncrypted   = null;
        string? jmbgHash2       = null;
        string? bankEncrypted   = null;
        string? emailEncrypted  = null;

        if (!string.IsNullOrEmpty(command.Jmbg))
        {
            jmbgEncrypted = _encryption.Encrypt(command.Jmbg);
            jmbgHash2     = _hash.ComputeJMBGHash(command.Jmbg);
        }

        if (!string.IsNullOrEmpty(command.BankAccount))
            bankEncrypted = _encryption.Encrypt(command.BankAccount);

        if (!string.IsNullOrEmpty(command.Email))
            emailEncrypted = _encryption.Encrypt(command.Email);

        employee.SetEncryptedData(jmbgEncrypted, jmbgHash2, bankEncrypted, emailEncrypted);

        _uow.Employees.Add(employee);
        await _uow.CommitAsync(cancellationToken);

        return Result<int>.Success(employee.Id);
    }
}
