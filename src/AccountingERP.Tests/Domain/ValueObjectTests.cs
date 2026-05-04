namespace AccountingERP.Tests.Domain;

using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

public class ValueObjectTests
{
    // ── PIB ───────────────────────────────────────────────────────────────────

    [Fact]
    public void PIB_WithNineDigits_ShouldBeValid()
    {
        var pib = new PIB("123456789");

        pib.Value.Should().Be("123456789");
    }

    [Fact]
    public void PIB_WithEightDigits_ShouldThrowDomainException()
    {
        var act = () => new PIB("12345678");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void PIB_WithLetters_ShouldThrowDomainException()
    {
        var act = () => new PIB("12345678A");

        act.Should().Throw<DomainException>();
    }

    // ── MaticniBroj ───────────────────────────────────────────────────────────

    [Fact]
    public void MaticniBroj_WithEightDigits_ShouldBeValid()
    {
        var mb = new MaticniBroj("12345678");

        mb.Value.Should().Be("12345678");
    }

    [Fact]
    public void MaticniBroj_WithSevenDigits_ShouldThrowDomainException()
    {
        var act = () => new MaticniBroj("1234567");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MaticniBroj_WithLetters_ShouldThrowDomainException()
    {
        var act = () => new MaticniBroj("1234567A");

        act.Should().Throw<DomainException>();
    }

    // ── AccountingPeriod ──────────────────────────────────────────────────────

    [Fact]
    public void AccountingPeriod_MonthZero_ShouldThrowDomainException()
    {
        var act = () => new AccountingPeriod(0, 2024);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AccountingPeriod_Month13_ShouldThrowDomainException()
    {
        var act = () => new AccountingPeriod(13, 2024);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void AccountingPeriod_ValidMonth_ShouldBeAccepted(int month)
    {
        var period = new AccountingPeriod(month, 2024);

        period.Month.Should().Be(month);
        period.Year.Should().Be(2024);
    }
}
