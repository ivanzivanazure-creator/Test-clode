namespace AccountingERP.Domain.Aggregates.JournalEntry;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.ValueObjects;

public class JournalLine : Entity<int>
{
    public int    AccountId { get; private set; }
    public Money  Debit     { get; private set; } = null!;
    public Money  Credit    { get; private set; } = null!;
    public string? Note     { get; private set; }

    private JournalLine() {}

    public static JournalLine Create(int accountId, Money debit, Money credit, string? note = null)
    {
        if (accountId <= 0) throw new DomainException("AccountId mora biti pozitivan");
        return new JournalLine { AccountId = accountId, Debit = debit, Credit = credit, Note = note };
    }
}
