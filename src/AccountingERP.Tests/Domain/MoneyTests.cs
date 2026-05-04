namespace AccountingERP.Tests.Domain;

using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

public class MoneyTests
{
    // ── Add ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        var a = Money.FromRSD(1500m);
        var b = Money.FromRSD(500m);

        var result = a.Add(b);

        result.Amount.Should().Be(2000m);
        result.Currency.Should().Be("RSD");
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrowDomainException()
    {
        var rsd = Money.FromRSD(1000m);
        var eur = new Money(100m, "EUR");

        var act = () => rsd.Add(eur);

        act.Should().Throw<DomainException>();
    }

    // ── Multiply ──────────────────────────────────────────────────────────────

    [Fact]
    public void Multiply_ShouldReturnCorrectProduct()
    {
        var price  = Money.FromRSD(400m);

        var result = price.Multiply(3m);

        result.Amount.Should().Be(1200m);
        result.Currency.Should().Be("RSD");
    }

    // ── VatRate ───────────────────────────────────────────────────────────────

    [Fact]
    public void VatRate_InvalidRate_ShouldThrowDomainException()
    {
        var act = () => new VatRate(15m);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0m)]
    [InlineData(8m)]
    [InlineData(10m)]
    [InlineData(20m)]
    public void VatRate_ValidRates_ShouldBeAccepted(decimal rate)
    {
        var vat = new VatRate(rate);

        vat.Percent.Should().Be(rate);
    }
}
