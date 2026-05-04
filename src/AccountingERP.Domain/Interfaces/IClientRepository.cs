namespace AccountingERP.Domain.Interfaces;

using AccountingERP.Domain.Aggregates.Client;

public interface IClientRepository : IRepository<Client, int>
{
    Task<IEnumerable<Client>> GetAllAsync(int tenantId, string? search, CancellationToken ct);
    Task<bool> PIBExistsAsync(int tenantId, string pib, CancellationToken ct);
}
