namespace AccountingERP.Application.Commands.Users;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record ChangePasswordCommand(
    int    Id,
    string CurrentPassword,
    string NewPassword) : IRequest<Result<Unit>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<Unit>>
{
    private readonly IUnitOfWork     _uow;
    private readonly IPasswordHasher _hasher;

    public ChangePasswordCommandHandler(IUnitOfWork uow, IPasswordHasher hasher)
    {
        _uow    = uow;
        _hasher = hasher;
    }

    public async Task<Result<Unit>> Handle(
        ChangePasswordCommand command,
        CancellationToken     cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(command.Id, cancellationToken);
        if (user is null)
            return Result<Unit>.Failure($"Korisnik sa ID={command.Id} nije pronađen.");

        if (!_hasher.Verify(command.CurrentPassword, user.PasswordHash))
            return Result<Unit>.Failure("Trenutna lozinka nije ispravna.");

        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < 6)
            return Result<Unit>.Failure("Nova lozinka mora imati najmanje 6 znakova.");

        var newHash = _hasher.Hash(command.NewPassword);
        user.UpdatePassword(newHash);

        await _uow.CommitAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
