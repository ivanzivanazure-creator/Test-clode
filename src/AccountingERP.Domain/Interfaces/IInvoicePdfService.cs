namespace AccountingERP.Domain.Interfaces;

using AccountingERP.Domain.Aggregates.Invoice;

public interface IInvoicePdfService
{
    byte[] Generate(Invoice invoice, string companyName, string companyPIB);
}
