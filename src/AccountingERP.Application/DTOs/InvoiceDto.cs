namespace AccountingERP.Application.DTOs;

using AccountingERP.Domain.Aggregates.Invoice;

/// <summary>Lightweight list-view DTO for an invoice.</summary>
public sealed record InvoiceDto(
    int            Id,
    int            TenantId,
    string         Number,
    int            ClientId,
    DateOnly       IssueDate,
    DateOnly       DueDate,
    string         Status,
    decimal        Amount,
    decimal        TaxAmount,
    decimal        TotalAmount,
    int            ItemCount)
{
    public static InvoiceDto FromDomain(Invoice inv) => new(
        Id:          inv.Id,
        TenantId:    inv.TenantId.Value,
        Number:      inv.Number,
        ClientId:    inv.ClientId,
        IssueDate:   inv.IssueDate,
        DueDate:     inv.DueDate,
        Status:      inv.Status.ToString(),
        Amount:      inv.Amount.Amount,
        TaxAmount:   inv.TaxAmount.Amount,
        TotalAmount: inv.TotalAmount.Amount,
        ItemCount:   inv.Items.Count);
}

/// <summary>Detailed DTO including the full list of line items.</summary>
public sealed record InvoiceDetailDto(
    int                      Id,
    int                      TenantId,
    string                   Number,
    int                      ClientId,
    DateOnly                 IssueDate,
    DateOnly                 DueDate,
    string                   Status,
    decimal                  Amount,
    decimal                  TaxAmount,
    decimal                  TotalAmount,
    IReadOnlyList<InvoiceItemDto> Items)
{
    public static InvoiceDetailDto FromDomain(Invoice inv) => new(
        Id:          inv.Id,
        TenantId:    inv.TenantId.Value,
        Number:      inv.Number,
        ClientId:    inv.ClientId,
        IssueDate:   inv.IssueDate,
        DueDate:     inv.DueDate,
        Status:      inv.Status.ToString(),
        Amount:      inv.Amount.Amount,
        TaxAmount:   inv.TaxAmount.Amount,
        TotalAmount: inv.TotalAmount.Amount,
        Items:       inv.Items.Select(InvoiceItemDto.FromDomain).ToList().AsReadOnly());
}

/// <summary>DTO for a single invoice line item.</summary>
public sealed record InvoiceItemDto(
    int     Id,
    string  Description,
    decimal Quantity,
    string  Unit,
    decimal UnitPrice,
    decimal VatPercent,
    decimal Total,
    decimal VatAmount,
    decimal TotalWithVat)
{
    public static InvoiceItemDto FromDomain(InvoiceItem item) => new(
        Id:           item.Id,
        Description:  item.Description,
        Quantity:     item.Quantity,
        Unit:         item.Unit,
        UnitPrice:    item.UnitPrice.Amount,
        VatPercent:   item.VatRate.Percent,
        Total:        item.Total.Amount,
        VatAmount:    item.VatAmount.Amount,
        TotalWithVat: item.TotalWithVat.Amount);
}
