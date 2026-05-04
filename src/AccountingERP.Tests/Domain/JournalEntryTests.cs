namespace AccountingERP.Tests.Domain;

using AccountingERP.Domain.Aggregates.JournalEntry;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

public class JournalEntryTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static TenantId Tenant => TenantId.From(1);

    private static JournalEntry MakeNacrt(string number = "KNJ-2024-001") =>
        JournalEntry.Create(
            Tenant,
            number,
            DateOnly.FromDateTime(DateTime.Today),
            "Testno knjiženje");

    // ── creation ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldBeInNacrtStatus()
    {
        var entry = MakeNacrt();

        entry.Status.Should().Be(JournalStatus.Nacrt);
        entry.Number.Should().Be("KNJ-2024-001");
    }

    // ── AddLine ───────────────────────────────────────────────────────────────

    [Fact]
    public void AddLine_WhenBothDebitAndCredit_ShouldThrowDomainException()
    {
        var entry = MakeNacrt();

        var act = () => entry.AddLine(
            accountId: 100,
            debit:  Money.FromRSD(1000m),
            credit: Money.FromRSD(1000m));

        act.Should().Throw<DomainException>();
    }

    // ── Post ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Post_WhenUnbalanced_ShouldThrowDomainException()
    {
        var entry = MakeNacrt();
        entry.AddLine(100, Money.FromRSD(1000m), Money.Zero);
        entry.AddLine(200, Money.Zero,            Money.FromRSD(900m));

        var act = () => entry.Post("user-1", "hash-abc", null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Post_WhenBalanced_ShouldChangeStatusToProknjizeno()
    {
        var entry = MakeNacrt();
        entry.AddLine(100, Money.FromRSD(1000m), Money.Zero);
        entry.AddLine(200, Money.Zero,            Money.FromRSD(1000m));

        entry.Post("user-1", "hash-abc", null);

        entry.Status.Should().Be(JournalStatus.Proknjiženo);
    }

    [Fact]
    public void Post_AlreadyPosted_ShouldThrowDomainException()
    {
        var entry = MakeNacrt();
        entry.AddLine(100, Money.FromRSD(500m), Money.Zero);
        entry.AddLine(200, Money.Zero,           Money.FromRSD(500m));
        entry.Post("user-1", "hash-abc", null);

        var act = () => entry.Post("user-1", "hash-xyz", "hash-abc");

        act.Should().Throw<DomainException>();
    }

    // ── CreateReversal ────────────────────────────────────────────────────────

    [Fact]
    public void CreateReversal_ShouldSwapDebitAndCredit()
    {
        var entry = MakeNacrt();
        entry.AddLine(100, Money.FromRSD(750m), Money.Zero);
        entry.AddLine(200, Money.Zero,           Money.FromRSD(750m));
        entry.Post("user-1", "hash-abc", null);

        var reversal = entry.CreateReversal("KNJ-STORNO-001");

        // Original line 1 was debit 750 — reversal should have credit 750 on same account
        reversal.Lines[0].Credit.Amount.Should().Be(750m);
        reversal.Lines[0].Debit.Amount.Should().Be(0m);

        // Original line 2 was credit 750 — reversal should have debit 750 on same account
        reversal.Lines[1].Debit.Amount.Should().Be(750m);
        reversal.Lines[1].Credit.Amount.Should().Be(0m);

        entry.Status.Should().Be(JournalStatus.Stornirano);
    }

    // ── IsBalanced ────────────────────────────────────────────────────────────

    [Fact]
    public void IsBalanced_ShouldReturnTrue_WhenDebitEqualsCredit()
    {
        var entry = MakeNacrt();
        entry.AddLine(100, Money.FromRSD(300m), Money.Zero);
        entry.AddLine(200, Money.Zero,           Money.FromRSD(300m));

        entry.IsBalanced.Should().BeTrue();
    }
}
