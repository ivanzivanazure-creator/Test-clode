namespace AccountingERP.Domain.Interfaces;

public interface IUnitOfWork
{
    IInvoiceRepository  Invoices  { get; }
    IJournalRepository  Journal   { get; }
    IEmployeeRepository Employees { get; }
    Task<int> CommitAsync(CancellationToken ct = default);
}
