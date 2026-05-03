namespace AccountingERP.Domain.ValueObjects;

public record VatRate
{
    public decimal Percent { get; }
    private static readonly decimal[] AllowedRates = { 0m, 8m, 10m, 20m };

    public VatRate(decimal percent)
    {
        if (!AllowedRates.Contains(percent))
            throw new DomainException($"PDV stopa {percent}% nije dozvoljena u RS");
        Percent = percent;
    }

    public static VatRate Standard => new(20);
    public static VatRate Reduced  => new(10);
    public static VatRate Zero     => new(0);

    public Money CalculateOn(Money base_) =>
        new(Math.Round(base_.Amount * Percent / 100, 2), base_.Currency);
}
