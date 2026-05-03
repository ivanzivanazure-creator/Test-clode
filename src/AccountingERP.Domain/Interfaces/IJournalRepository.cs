namespace AccountingERP.Domain.Interfaces;

using AccountingERP.Domain.Aggregates.JournalEntry;
using AccountingERP.Domain.ValueObjects;

public interface IJournalRepository : IRepository<JournalEntry, int>
{
    Task<string?>                    GetLastHashAsync(int tenantId, CancellationToken ct);
    Task<bool>                       IsPeriodLockedAsync(int tenantId, int month, int year, CancellationToken ct);
    Task<IEnumerable<AccountBalance>> GetBalancesAsync(int tenantId, DateOnly asOf, CancellationToken ct);
}

public record AccountBalance(string Code, string Name, string Type, Money Balance);
