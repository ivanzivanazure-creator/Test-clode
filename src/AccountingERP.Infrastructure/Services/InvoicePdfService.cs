namespace AccountingERP.Infrastructure.Services;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AccountingERP.Domain.Aggregates.Invoice;
using AccountingERP.Domain.Interfaces;

public class InvoicePdfService : IInvoicePdfService
{
    public byte[] Generate(Invoice invoice, string companyName, string companyPIB)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(companyName).Bold().FontSize(18).FontColor("#1e40af");
                            c.Item().Text($"PIB: {companyPIB}").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                        row.ConstantItem(150).AlignRight().Column(c =>
                        {
                            c.Item().Text("FAKTURA").Bold().FontSize(20).FontColor("#1e40af");
                            c.Item().Text(invoice.Number).Bold().FontSize(14);
                        });
                    });
                    col.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Light).Row(_ => { });
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    // Invoice meta
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Datum izdavanja:").SemiBold();
                            c.Item().Text(invoice.IssueDate.ToString("dd.MM.yyyy."));
                            c.Item().PaddingTop(5).Text("Valuta plaćanja:").SemiBold();
                            c.Item().Text(invoice.DueDate.ToString("dd.MM.yyyy."));
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Status:").SemiBold();
                            c.Item().Text(invoice.Status.ToString());
                        });
                    });

                    // Items table
                    col.Item().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        // Header
                        static IContainer HeaderCell(IContainer c) =>
                            c.Background("#1e40af").Padding(6).DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold());

                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("Opis");
                            h.Cell().Element(HeaderCell).AlignCenter().Text("Kol.");
                            h.Cell().Element(HeaderCell).AlignRight().Text("Jed. cijena");
                            h.Cell().Element(HeaderCell).AlignCenter().Text("PDV %");
                            h.Cell().Element(HeaderCell).AlignRight().Text("PDV");
                            h.Cell().Element(HeaderCell).AlignRight().Text("Ukupno");
                        });

                        bool alt = false;
                        foreach (var item in invoice.Items)
                        {
                            var bg = alt ? "#f8fafc" : Colors.White;
                            alt = !alt;
                            static IContainer Cell(IContainer c, string bg) => c.Background(bg).Padding(5);

                            table.Cell().Element(c => Cell(c, bg)).Text(item.Description);
                            table.Cell().Element(c => Cell(c, bg)).AlignCenter().Text($"{item.Quantity:N2} {item.Unit}");
                            table.Cell().Element(c => Cell(c, bg)).AlignRight().Text($"{item.UnitPrice.Amount:N2}");
                            table.Cell().Element(c => Cell(c, bg)).AlignCenter().Text($"{item.VatRate.Percent}%");
                            table.Cell().Element(c => Cell(c, bg)).AlignRight().Text($"{item.VatAmount.Amount:N2}");
                            table.Cell().Element(c => Cell(c, bg)).AlignRight().Text($"{item.TotalWithVat.Amount:N2}");
                        }
                    });

                    // Totals
                    col.Item().PaddingTop(10).AlignRight().Column(totals =>
                    {
                        totals.Item().Row(r =>
                        {
                            r.ConstantItem(120).Text("Osnovica:").AlignRight();
                            r.ConstantItem(100).Text($"{invoice.Amount.Amount:N2} RSD").AlignRight();
                        });
                        totals.Item().Row(r =>
                        {
                            r.ConstantItem(120).Text("PDV:").AlignRight();
                            r.ConstantItem(100).Text($"{invoice.TaxAmount.Amount:N2} RSD").AlignRight();
                        });
                        totals.Item().BorderTop(2).BorderColor("#1e40af").Row(r =>
                        {
                            r.ConstantItem(120).Text("UKUPNO:").Bold().AlignRight();
                            r.ConstantItem(100).Text($"{invoice.TotalAmount.Amount:N2} RSD").Bold().FontColor("#1e40af").AlignRight();
                        });
                    });
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Stranica ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.CurrentPageNumber().FontSize(8);
                    t.Span(" od ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.TotalPages().FontSize(8);
                });
            });
        }).GeneratePdf();
    }
}
