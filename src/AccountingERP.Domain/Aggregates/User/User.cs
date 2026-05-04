namespace AccountingERP.Domain.Aggregates.User;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.Exceptions;

public enum UserRole { Admin, Accountant, Viewer }

public class User : AggregateRoot<int>
{
    public int      TenantId     { get; private set; }
    public string   Username     { get; private set; } = null!;
    public string   PasswordHash { get; private set; } = null!;
    public string   Email        { get; private set; } = null!;
    public string   FullName     { get; private set; } = null!;
    public UserRole Role         { get; private set; }
    public bool     IsActive     { get; private set; }

    private User() { }

    public static User Create(
        int      tenantId,
        string   username,
        string   passwordHash,
        string   email,
        string   fullName,
        UserRole role)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Korisničko ime je obavezno.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email je obavezan.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Puno ime je obavezno.");

        return new User
        {
            TenantId     = tenantId,
            Username     = username.Trim(),
            PasswordHash = passwordHash,
            Email        = email.Trim(),
            FullName     = fullName.Trim(),
            Role         = role,
            IsActive     = true,
        };
    }

    public void UpdatePassword(string newHash)
    {
        if (string.IsNullOrWhiteSpace(newHash))
            throw new DomainException("Hash lozinke ne može biti prazan.");

        PasswordHash = newHash;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
