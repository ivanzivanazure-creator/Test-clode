namespace AccountingERP.Domain.ValueObjects;

public record TenantId(int Value)
{
    public static TenantId From(int value)
    {
        if (value <= 0) throw new DomainException("TenantId mora biti pozitivan broj");
        return new TenantId(value);
    }
}
