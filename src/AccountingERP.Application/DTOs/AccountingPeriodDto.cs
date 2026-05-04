namespace AccountingERP.Application.DTOs;

using AccountingERP.Domain.Aggregates.AccountingPeriod;

public sealed record AccountingPeriodDto(
    int       Id,
    int       Month,
    int       Year,
    bool      IsLocked,
    DateTime? LockedAt,
    string?   LockedByUserId)
{
    public static AccountingPeriodDto FromDomain(AccountingPeriod p) => new(
        p.Id, p.Month, p.Year, p.IsLocked, p.LockedAt, p.LockedByUserId);
}
