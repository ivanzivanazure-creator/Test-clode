namespace AccountingERP.Domain.Aggregates.AccountingPeriod;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.Events;
using AccountingERP.Domain.Exceptions;

public class AccountingPeriod : AggregateRoot<int>
{
    public int      TenantId        { get; private set; }
    public int      Month           { get; private set; }
    public int      Year            { get; private set; }
    public bool     IsLocked        { get; private set; }
    public DateTime? LockedAt       { get; private set; }
    public string?  LockedByUserId  { get; private set; }

    private AccountingPeriod() { }

    public static AccountingPeriod Create(int tenantId, int month, int year)
    {
        if (month < 1 || month > 12)
            throw new DomainException("Mesec mora biti između 1 i 12.");
        if (year < 2000 || year > 2100)
            throw new DomainException("Godina nije validna.");

        return new AccountingPeriod
        {
            TenantId = tenantId,
            Month    = month,
            Year     = year,
            IsLocked = false,
        };
    }

    public void Lock(string userId)
    {
        if (IsLocked)
            throw new DomainException($"Period {Month:D2}/{Year} je već zaključan.");

        IsLocked       = true;
        LockedAt       = DateTime.UtcNow;
        LockedByUserId = userId;

        Raise(new PeriodLockedEvent(TenantId, Id, Month, Year, userId));
    }

    public void Unlock(string userId)
    {
        if (!IsLocked)
            throw new DomainException($"Period {Month:D2}/{Year} nije zaključan.");

        IsLocked       = false;
        LockedAt       = null;
        LockedByUserId = null;

        Raise(new PeriodUnlockedEvent(TenantId, Id, Month, Year, userId));
    }
}
