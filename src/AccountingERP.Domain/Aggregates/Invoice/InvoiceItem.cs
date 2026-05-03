namespace AccountingERP.Domain.Aggregates.Invoice;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.ValueObjects;

public class InvoiceItem : Entity<int>
{
    public string  Description { get; private set; } = null!;
    public decimal Quantity    { get; private set; }
    public Money   UnitPrice   { get; private set; } = null!;
    public VatRate VatRate     { get; private set; } = null!;
    public string  Unit        { get; private set; } = "kom";

    public Money Total        => UnitPrice.Multiply(Quantity);
    public Money VatAmount    => VatRate.CalculateOn(Total);
    public Money TotalWithVat => Total.Add(VatAmount);

    private InvoiceItem() {}

    public static InvoiceItem Create(
        string description, decimal quantity, Money unitPrice, VatRate vatRate, string unit = "kom")
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Opis stavke je obavezan");
        if (quantity <= 0)
            throw new DomainException("Količina mora biti pozitivna");
        if (!unitPrice.IsPositive)
            throw new DomainException("Cijena mora biti pozitivna");

        return new InvoiceItem
        {
            Description = description.Trim(),
            Quantity    = quantity,
            UnitPrice   = unitPrice,
            VatRate     = vatRate,
            Unit        = unit,
        };
    }
}
