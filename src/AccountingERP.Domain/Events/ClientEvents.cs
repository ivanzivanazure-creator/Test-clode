namespace AccountingERP.Domain.Events;

using AccountingERP.Domain.Aggregates.Client;
using AccountingERP.Domain.Common;

public record ClientCreatedEvent(int TenantId, string Name, ClientType ClientType) : DomainEvent;
public record ClientUpdatedEvent(int TenantId, int ClientId, string Name) : DomainEvent;
