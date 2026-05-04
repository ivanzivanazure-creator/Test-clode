namespace AccountingERP.Domain.Interfaces;

using AccountingERP.Domain.Aggregates.User;

public interface IUserRepository : IRepository<User, int>
{
    Task<User?> GetByUsernameAsync(int tenantId, string username, CancellationToken ct);
    Task<IEnumerable<User>> GetAllAsync(int tenantId, CancellationToken ct);
}
