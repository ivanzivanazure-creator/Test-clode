namespace AccountingERP.Domain.ValueObjects;

public record Money(decimal Amount, string Currency = "RSD")
{
    public static Money Zero => new(0);
    public static Money FromRSD(decimal amount) => new(amount, "RSD");

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Ne može se sabirati {Currency} i {other.Currency}");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);
    public Money WithVat(decimal vatRate) => new(Amount + Amount * vatRate / 100, Currency);
    public Money VatAmount(decimal vatRate) => new(Amount * vatRate / 100, Currency);

    public bool IsPositive => Amount > 0;
    public bool IsZero     => Amount == 0;

    public override string ToString() => $"{Amount:N2} {Currency}";
}
