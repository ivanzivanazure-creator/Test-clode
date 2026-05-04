namespace AccountingERP.Application.DTOs;

using AccountingERP.Domain.Aggregates.User;

public sealed record UserDto(
    int      Id,
    string   Username,
    string   Email,
    string   FullName,
    UserRole Role,
    bool     IsActive)
{
    public static UserDto FromDomain(User u) => new(
        u.Id, u.Username, u.Email, u.FullName, u.Role, u.IsActive);
}
