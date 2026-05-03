namespace AccountingERP.Domain.Events;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.ValueObjects;

public record JournalEntryPostedEvent(int TenantId, int EntryId, string Number, DateOnly Date, Money TotalDebit) : DomainEvent;
public record JournalEntryReversedEvent(int TenantId, int OriginalId, string ReversalNumber) : DomainEvent;
