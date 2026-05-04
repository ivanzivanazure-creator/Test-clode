namespace AccountingERP.Infrastructure.Services;

using AccountingERP.Domain.Interfaces;
using BC = BCrypt.Net.BCrypt;

/// <summary>
/// BCrypt-based password hasher using BCrypt.Net-Next.
/// Work factor 12 is a good default: ~300ms per hash on modern hardware,
/// slow enough to resist brute-force and fast enough for interactive login.
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
        => BC.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
        => BC.Verify(password, hash);
}
