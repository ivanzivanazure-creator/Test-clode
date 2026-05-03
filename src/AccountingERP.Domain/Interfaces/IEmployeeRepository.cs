namespace AccountingERP.Domain.Interfaces;

using AccountingERP.Domain.Aggregates.Employee;

public interface IEmployeeRepository : IRepository<Employee, int>
{
    Task<IEnumerable<Employee>> GetActiveAsync(int tenantId, CancellationToken ct);
    Task<Employee?>             GetByJMBGHashAsync(int tenantId, string jmbgHash, CancellationToken ct);
}
