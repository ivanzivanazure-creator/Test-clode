namespace AccountingERP.Application.Commands.Users;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Aggregates.User;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using FluentValidation;
using MediatR;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record CreateUserCommand(
    int      TenantId,
    string   Username,
    string   Password,
    string   Email,
    string   FullName,
    UserRole Role) : IRequest<Result<int>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("TenantId mora biti pozitivan broj.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Korisničko ime je obavezno.")
            .MinimumLength(3).WithMessage("Korisničko ime mora imati najmanje 3 znaka.")
            .MaximumLength(100).WithMessage("Korisničko ime ne sme biti duže od 100 znakova.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Lozinka je obavezna.")
            .MinimumLength(6).WithMessage("Lozinka mora imati najmanje 6 znakova.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email je obavezan.")
            .EmailAddress().WithMessage("Email adresa nije ispravna.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Puno ime je obavezno.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<int>>
{
    private readonly IUnitOfWork     _uow;
    private readonly IPasswordHasher _hasher;

    public CreateUserCommandHandler(IUnitOfWork uow, IPasswordHasher hasher)
    {
        _uow    = uow;
        _hasher = hasher;
    }

    public async Task<Result<int>> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var existing = await _uow.Users.GetByUsernameAsync(
            command.TenantId, command.Username, cancellationToken);

        if (existing is not null)
            return Result<int>.Failure(
                $"Korisnik sa korisničkim imenom '{command.Username}' već postoji.");

        var hash = _hasher.Hash(command.Password);

        User user;
        try
        {
            user = User.Create(command.TenantId, command.Username, hash,
                               command.Email, command.FullName, command.Role);
        }
        catch (DomainException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        _uow.Users.Add(user);
        await _uow.CommitAsync(cancellationToken);

        return Result<int>.Success(user.Id);
    }
}
