namespace AccountingERP.Domain.Interfaces;

public interface IUnitOfWork
{
    IInvoiceRepository          Invoices  { get; }
    IJournalRepository          Journal   { get; }
    IEmployeeRepository         Employees { get; }
    IClientRepository           Clients   { get; }
    IAccountingPeriodRepository Periods   { get; }
    IUserRepository             Users     { get; }
    Task<int> CommitAsync(CancellationToken ct = default);
}
