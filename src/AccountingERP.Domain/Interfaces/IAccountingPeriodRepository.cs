namespace AccountingERP.Domain.Interfaces;

using AccountingERP.Domain.Aggregates.AccountingPeriod;

public interface IAccountingPeriodRepository : IRepository<AccountingPeriod, int>
{
    Task<IEnumerable<AccountingPeriod>> GetByYearAsync(int tenantId, int year, CancellationToken ct);
    Task<AccountingPeriod?> GetAsync(int tenantId, int month, int year, CancellationToken ct);
}
