namespace AccountingERP.Domain.ValueObjects;

public record PIB
{
    public string Value { get; }

    public PIB(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 9 || !value.All(char.IsDigit))
            throw new DomainException($"PIB '{value}' nije validan — mora biti 9 cifara");
        Value = value;
    }

    public override string ToString() => Value;
    public static implicit operator string(PIB pib) => pib.Value;
}

public record MaticniBroj
{
    public string Value { get; }

    public MaticniBroj(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 8 || !value.All(char.IsDigit))
            throw new DomainException($"MB '{value}' nije validan — mora biti 8 cifara");
        Value = value;
    }
}

public record IBAN
{
    public string Value { get; }

    public IBAN(string value)
    {
        var clean = value?.Replace(" ", "").ToUpperInvariant() ?? "";
        if (clean.Length < 15 || clean.Length > 34)
            throw new DomainException("IBAN nije validan");
        Value = clean;
    }
}

public record EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
            throw new DomainException($"Email '{value}' nije validan");
        Value = value.Trim().ToLowerInvariant();
    }
}
