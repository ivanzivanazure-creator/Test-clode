namespace AccountingERP.Application.Commands.Periods;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record UnlockPeriodCommand(
    int    TenantId,
    int    Id,
    string UserId) : IRequest<Result<Unit>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class UnlockPeriodCommandHandler : IRequestHandler<UnlockPeriodCommand, Result<Unit>>
{
    private readonly IUnitOfWork _uow;

    public UnlockPeriodCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Unit>> Handle(
        UnlockPeriodCommand command,
        CancellationToken   cancellationToken)
    {
        var period = await _uow.Periods.GetByIdAsync(command.Id, cancellationToken);
        if (period is null)
            return Result<Unit>.Failure($"Period sa ID={command.Id} nije pronađen.");

        try
        {
            period.Unlock(command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }

        await _uow.CommitAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
