namespace AccountingERP.Domain.Events;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.ValueObjects;

public record EmployeeCreatedEvent(int TenantId, int EmployeeId, string FirstName, string LastName) : DomainEvent;
public record EmployeeSalaryChangedEvent(int TenantId, int EmployeeId, Money OldSalary, Money NewSalary, string Reason) : DomainEvent;
public record EmployeeTerminatedEvent(int TenantId, int EmployeeId, DateOnly TerminationDate) : DomainEvent;
public record EmployeePseudonymizedEvent(int TenantId, int EmployeeId, string Reason) : DomainEvent;
