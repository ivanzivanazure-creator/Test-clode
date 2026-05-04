namespace AccountingERP.Application.Commands.Employees;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using FluentValidation;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Terminates an active employee as of the given date.
/// The termination date must not precede the employee's hire date.
/// An <see cref="Domain.Events.EmployeeTerminatedEvent"/> is raised for downstream
/// processing (e.g. payroll proration, HR notifications).
/// </summary>
public sealed record TerminateEmployeeCommand(
    int      EmployeeId,
    DateOnly TerminationDate) : IRequest<Result<Unit>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class TerminateEmployeeCommandValidator : AbstractValidator<TerminateEmployeeCommand>
{
    public TerminateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("EmployeeId mora biti pozitivan broj.");

        RuleFor(x => x.TerminationDate)
            .NotEmpty().WithMessage("Datum prestanka radnog odnosa je obavezan.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today).AddDays(90))
            .WithMessage("Datum prestanka ne može biti više od 90 dana u budućnosti.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class TerminateEmployeeCommandHandler
    : IRequestHandler<TerminateEmployeeCommand, Result<Unit>>
{
    private readonly IUnitOfWork _uow;

    public TerminateEmployeeCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Unit>> Handle(
        TerminateEmployeeCommand command,
        CancellationToken        cancellationToken)
    {
        var employee = await _uow.Employees.GetByIdAsync(command.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<Unit>.Failure($"Zaposleni sa ID={command.EmployeeId} nije pronađen.");

        try
        {
            employee.Terminate(command.TerminationDate);
        }
        catch (DomainException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }

        await _uow.CommitAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
