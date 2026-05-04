namespace AccountingERP.Infrastructure.Repositories;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Infrastructure.Persistence;
using MediatR;

/// <summary>
/// Coordinates repository operations under a single DbContext transaction.
/// After persisting changes, dispatches any domain events raised by aggregates
/// through MediatR so that event handlers run within the same request pipeline.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private readonly IPublisher   _publisher;

    public IInvoiceRepository  Invoices  { get; }
    public IJournalRepository  Journal   { get; }
    public IEmployeeRepository Employees { get; }

    public UnitOfWork(
        AppDbContext        db,
        IPublisher          publisher,
        IInvoiceRepository  invoices,
        IJournalRepository  journal,
        IEmployeeRepository employees)
    {
        _db        = db;
        _publisher = publisher;
        Invoices   = invoices;
        Journal    = journal;
        Employees  = employees;
    }

    /// <summary>
    /// Saves all pending changes then dispatches domain events collected on aggregates.
    /// Domain events are dispatched after the database write so that handlers can
    /// safely read the persisted state (e.g. generated IDs).
    /// </summary>
    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        // Collect domain events before clearing them.
        var domainEvents = _db.ChangeTracker
            .Entries<AggregateRoot<int>>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var affected = await _db.SaveChangesAsync(ct);

        // Clear events on all tracked aggregates so they are not re-dispatched.
        foreach (var entry in _db.ChangeTracker.Entries<AggregateRoot<int>>())
            entry.Entity.ClearEvents();

        // Dispatch each domain event through MediatR.
        // Using IPublisher (fire-and-forget style) keeps handlers decoupled.
        foreach (var domainEvent in domainEvents)
        {
            if (domainEvent is INotification notification)
                await _publisher.Publish(notification, ct);
        }

        return affected;
    }
}
