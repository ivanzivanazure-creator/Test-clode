namespace AccountingERP.Tests.Domain;

using AccountingERP.Domain.Aggregates.Invoice;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

public class InvoiceTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static TenantId Tenant => TenantId.From(1);

    private static IEnumerable<(string, decimal, Money, VatRate)> OneItem(
        decimal unitPrice = 1000m,
        decimal vatPct    = 20m) =>
        [("Usluga razvoja", 1m, Money.FromRSD(unitPrice), new VatRate(vatPct))];

    private static Invoice MakeNacrt(
        string number  = "INV-2024-001",
        DateOnly? due  = null,
        IEnumerable<(string, decimal, Money, VatRate)>? items = null) =>
        Invoice.Create(
            Tenant,
            number,
            clientId: 42,
            due ?? DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            items ?? OneItem());

    // ── creation ─────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldReturnNacrtStatus()
    {
        var items = new[]
        {
            ("Usluga A", 2m, Money.FromRSD(500m), new VatRate(20m)),
            ("Usluga B", 1m, Money.FromRSD(300m), new VatRate(10m)),
        };

        var inv = Invoice.Create(Tenant, "INV-2024-001", 42,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)), items);

        inv.Status.Should().Be(InvoiceStatus.Nacrt);
        inv.Number.Should().Be("INV-2024-001");
        inv.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Create_WithEmptyNumber_ShouldThrowDomainException()
    {
        var act = () => Invoice.Create(Tenant, "   ", 42,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)), OneItem());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNoItems_ShouldThrowDomainException()
    {
        var act = () => Invoice.Create(Tenant, "INV-2024-001", 42,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            Array.Empty<(string, decimal, Money, VatRate)>());

        act.Should().Throw<DomainException>();
    }

    // ── Issue ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Issue_FromNacrt_ShouldChangeStatusToIzdata()
    {
        var inv = MakeNacrt();

        inv.Issue("user-1");

        inv.Status.Should().Be(InvoiceStatus.Izdata);
    }

    [Fact]
    public void Issue_FromIzdata_ShouldThrowDomainException()
    {
        var inv = MakeNacrt();
        inv.Issue("user-1");

        var act = () => inv.Issue("user-1");

        act.Should().Throw<DomainException>();
    }

    // ── MarkPaid ──────────────────────────────────────────────────────────────

    [Fact]
    public void MarkPaid_FromIzdata_ShouldChangeStatusToPlaćena()
    {
        var inv = MakeNacrt();
        inv.Issue("user-1");

        inv.MarkPaid(DateOnly.FromDateTime(DateTime.Today));

        inv.Status.Should().Be(InvoiceStatus.Plaćena);
    }

    [Fact]
    public void MarkPaid_FromNacrt_ShouldThrowDomainException()
    {
        var inv = MakeNacrt();

        var act = () => inv.MarkPaid(DateOnly.FromDateTime(DateTime.Today));

        act.Should().Throw<DomainException>();
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_PaidInvoice_ShouldThrowDomainException()
    {
        var inv = MakeNacrt();
        inv.Issue("user-1");
        inv.MarkPaid(DateOnly.FromDateTime(DateTime.Today));

        var act = () => inv.Cancel();

        act.Should().Throw<DomainException>();
    }

    // ── TotalAmount ───────────────────────────────────────────────────────────

    [Fact]
    public void TotalAmount_ShouldEqualSumOfItemsWithVat()
    {
        // Item 1: qty=2, unitPrice=500 RSD, VAT 20%  => base 1000, vat 200, total 1200
        // Item 2: qty=1, unitPrice=800 RSD, VAT 10%  => base  800, vat  80, total  880
        // Grand total: 2080
        var items = new[]
        {
            ("Usluga A", 2m, Money.FromRSD(500m), new VatRate(20m)),
            ("Usluga B", 1m, Money.FromRSD(800m), new VatRate(10m)),
        };

        var inv = Invoice.Create(Tenant, "INV-2024-002", 42,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)), items);

        inv.TotalAmount.Amount.Should().Be(2080m);
    }

    // ── MarkOverdue ───────────────────────────────────────────────────────────

    [Fact]
    public void MarkOverdue_WhenDueDatePassed_ShouldChangeToDospela()
    {
        // Due date in the past
        var pastDue = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));
        var inv     = MakeNacrt(due: pastDue);
        inv.Issue("user-1");

        inv.MarkOverdue();

        inv.Status.Should().Be(InvoiceStatus.Dospela);
    }
}
