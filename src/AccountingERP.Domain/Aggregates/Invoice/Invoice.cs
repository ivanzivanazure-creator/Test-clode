namespace AccountingERP.Domain.Aggregates.Invoice;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.Events;
using AccountingERP.Domain.ValueObjects;

public enum InvoiceStatus { Nacrt, Izdata, Plaćena, Dospela, Otkazana }

public class Invoice : AggregateRoot<int>
{
    public TenantId      TenantId   { get; private set; } = null!;
    public string        Number     { get; private set; } = null!;
    public int           ClientId   { get; private set; }
    public DateOnly      IssueDate  { get; private set; }
    public DateOnly      DueDate    { get; private set; }
    public InvoiceStatus Status     { get; private set; }

    private readonly List<InvoiceItem> _items = new();
    public IReadOnlyList<InvoiceItem> Items => _items.AsReadOnly();

    public Money Amount      => Money.FromRSD(_items.Sum(i => i.Total.Amount));
    public Money TaxAmount   => Money.FromRSD(_items.Sum(i => i.VatAmount.Amount));
    public Money TotalAmount => Amount.Add(TaxAmount);

    // Integritet (Zakon o računovodstvu čl. 8)
    public string? IntegrityHash { get; private set; }
    public DateOnly RetainUntil  => IssueDate.AddYears(10);

    private Invoice() {}

    public static Invoice Create(
        TenantId tenantId,
        string number,
        int clientId,
        DateOnly dueDate,
        IEnumerable<(string description, decimal qty, Money unitPrice, VatRate vatRate)> items)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Broj fakture je obavezan");
        if (!items.Any())
            throw new DomainException("Faktura mora imati najmanje jednu stavku");

        var inv = new Invoice
        {
            TenantId  = tenantId,
            Number    = number,
            ClientId  = clientId,
            IssueDate = DateOnly.FromDateTime(DateTime.Today),
            DueDate   = dueDate,
            Status    = InvoiceStatus.Nacrt,
        };

        foreach (var (desc, qty, price, vat) in items)
            inv._items.Add(InvoiceItem.Create(desc, qty, price, vat));

        inv.Raise(new InvoiceCreatedEvent(inv.TenantId.Value, inv.Number, inv.TotalAmount));
        return inv;
    }

    public void Issue(string issuedByUserId)
    {
        if (Status != InvoiceStatus.Nacrt)
            throw new DomainException($"Faktura {Number} je u statusu {Status} — ne može se izdati");
        if (!_items.Any())
            throw new DomainException("Faktura nema stavki");

        Status = InvoiceStatus.Izdata;
        Raise(new InvoiceIssuedEvent(TenantId.Value, Id, Number, TotalAmount, DueDate, issuedByUserId));
    }

    public void MarkPaid(DateOnly paymentDate)
    {
        if (Status != InvoiceStatus.Izdata && Status != InvoiceStatus.Dospela)
            throw new DomainException($"Faktura {Number} nije u stanju za naplatu");
        if (paymentDate > DateOnly.FromDateTime(DateTime.Today).AddDays(1))
            throw new DomainException("Datum naplate ne može biti u budućnosti");

        Status = InvoiceStatus.Plaćena;
        Raise(new InvoicePaidEvent(TenantId.Value, Id, Number, TotalAmount, paymentDate));
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Plaćena)
            throw new DomainException("Plaćena faktura ne može biti otkazana");
        if (Status == InvoiceStatus.Otkazana)
            throw new DomainException("Faktura je već otkazana");

        Status = InvoiceStatus.Otkazana;
        Raise(new InvoiceCancelledEvent(TenantId.Value, Id, Number));
    }

    public void SetIntegrityHash(string hash)
    {
        if (!string.IsNullOrEmpty(IntegrityHash))
            throw new DomainException("Integritetni hash već postoji");
        IntegrityHash = hash;
    }

    public void MarkOverdue()
    {
        if (Status == InvoiceStatus.Izdata && DueDate < DateOnly.FromDateTime(DateTime.Today))
        {
            Status = InvoiceStatus.Dospela;
            Raise(new InvoiceOverdueEvent(TenantId.Value, Id, Number, TotalAmount, DueDate));
        }
    }
}
