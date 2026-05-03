namespace AccountingERP.Domain.Interfaces;

using AccountingERP.Domain.Common;

public interface IRepository<T, TId> where T : AggregateRoot<TId>
{
    Task<T?>  GetByIdAsync(TId id, CancellationToken ct = default);
    Task<T>   GetByIdOrThrowAsync(TId id, CancellationToken ct = default);
    void      Add(T entity);
    void      Remove(T entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
