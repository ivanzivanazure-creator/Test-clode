namespace AccountingERP.Domain.ValueObjects;

public record AccountingPeriod
{
    public int Month { get; }
    public int Year  { get; }

    public AccountingPeriod(int month, int year)
    {
        if (month < 1 || month > 12) throw new DomainException("Mesec mora biti 1-12");
        if (year < 2000 || year > 2100) throw new DomainException("Godina nije validna");
        Month = month;
        Year  = year;
    }

    public static AccountingPeriod Current =>
        new(DateTime.Today.Month, DateTime.Today.Year);

    public bool Contains(DateOnly date) =>
        date.Month == Month && date.Year == Year;

    public override string ToString() => $"{Month:D2}/{Year}";
}
