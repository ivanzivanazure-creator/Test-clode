namespace AccountingERP.Application.Commands.Employees;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Domain.ValueObjects;
using FluentValidation;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Updates the gross salary of an active employee.
/// A mandatory reason string is required for the audit trail and is
/// carried in the <see cref="Domain.Events.EmployeeSalaryChangedEvent"/>.
/// </summary>
public sealed record UpdateSalaryCommand(
    int     EmployeeId,
    decimal NewGrossSalary,
    string  Reason) : IRequest<Result<Unit>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class UpdateSalaryCommandValidator : AbstractValidator<UpdateSalaryCommand>
{
    public UpdateSalaryCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("EmployeeId mora biti pozitivan broj.");

        RuleFor(x => x.NewGrossSalary)
            .GreaterThan(0).WithMessage("Nova bruto plata mora biti veća od nule.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Razlog promene plate je obavezan.")
            .MaximumLength(500).WithMessage("Razlog ne sme biti duži od 500 znakova.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class UpdateSalaryCommandHandler : IRequestHandler<UpdateSalaryCommand, Result<Unit>>
{
    private readonly IUnitOfWork _uow;

    public UpdateSalaryCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Unit>> Handle(
        UpdateSalaryCommand command,
        CancellationToken   cancellationToken)
    {
        var employee = await _uow.Employees.GetByIdAsync(command.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<Unit>.Failure($"Zaposleni sa ID={command.EmployeeId} nije pronađen.");

        if (!employee.IsActive)
            return Result<Unit>.Failure("Nije moguće promeniti platu neaktivnom zaposlenom.");

        try
        {
            employee.UpdateSalary(Money.FromRSD(command.NewGrossSalary), command.Reason);
        }
        catch (DomainException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }

        await _uow.CommitAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
