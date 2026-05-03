namespace AccountingERP.Domain.Events;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.ValueObjects;

public record InvoiceCreatedEvent(int TenantId, string Number, Money TotalAmount) : DomainEvent;
public record InvoiceIssuedEvent(int TenantId, int InvoiceId, string Number, Money TotalAmount, DateOnly DueDate, string IssuedByUserId) : DomainEvent;
public record InvoicePaidEvent(int TenantId, int InvoiceId, string Number, Money TotalAmount, DateOnly PaidDate) : DomainEvent;
public record InvoiceCancelledEvent(int TenantId, int InvoiceId, string Number) : DomainEvent;
public record InvoiceOverdueEvent(int TenantId, int InvoiceId, string Number, Money TotalAmount, DateOnly DueDate) : DomainEvent;
