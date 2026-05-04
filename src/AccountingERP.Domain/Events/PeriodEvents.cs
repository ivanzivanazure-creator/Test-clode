namespace AccountingERP.Domain.Events;

using AccountingERP.Domain.Common;

public record PeriodLockedEvent(int TenantId, int PeriodId, int Month, int Year, string LockedByUserId) : DomainEvent;
public record PeriodUnlockedEvent(int TenantId, int PeriodId, int Month, int Year, string UnlockedByUserId) : DomainEvent;
