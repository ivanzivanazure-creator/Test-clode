namespace AccountingERP.Domain.Aggregates.JournalEntry;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.Events;
using AccountingERP.Domain.ValueObjects;

public enum JournalStatus { Nacrt, Proknjiženo, Stornirano }

public class JournalEntry : AggregateRoot<int>
{
    public TenantId      TenantId    { get; private set; } = null!;
    public string        Number      { get; private set; } = null!;
    public DateOnly      Date        { get; private set; }
    public string        Description { get; private set; } = null!;
    public JournalStatus Status      { get; private set; }
    public string?       SourceType  { get; private set; }
    public int?          SourceId    { get; private set; }

    private readonly List<JournalLine> _lines = new();
    public IReadOnlyList<JournalLine> Lines => _lines.AsReadOnly();

    public Money TotalDebit  => Money.FromRSD(_lines.Sum(l => l.Debit.Amount));
    public Money TotalCredit => Money.FromRSD(_lines.Sum(l => l.Credit.Amount));
    public bool  IsBalanced  => Math.Abs(TotalDebit.Amount - TotalCredit.Amount) < 0.01m;

    // Zakon o računovodstvu — SHA-256 hash lanac
    public string?   IntegrityHash  { get; private set; }
    public string?   PreviousHash   { get; private set; }
    public string?   PostedByUserId { get; private set; }
    public DateTime? PostedAtUtc    { get; private set; }
    public DateOnly  RetainUntil    => Date.AddYears(10);

    private JournalEntry() {}

    public static JournalEntry Create(
        TenantId tenantId,
        string number,
        DateOnly date,
        string description,
        string? sourceType = null,
        int? sourceId = null)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Broj knjiženja je obavezan");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Opis knjiženja je obavezan");

        return new JournalEntry
        {
            TenantId    = tenantId,
            Number      = number,
            Date        = date,
            Description = description,
            Status      = JournalStatus.Nacrt,
            SourceType  = sourceType,
            SourceId    = sourceId,
        };
    }

    public void AddLine(int accountId, Money debit, Money credit, string? note = null)
    {
        if (Status != JournalStatus.Nacrt)
            throw new DomainException("Ne može se dodati stavka u proknjiženu temeljnicu");
        if (debit.Amount > 0 && credit.Amount > 0)
            throw new DomainException("Stavka ne može imati i duguje i potražuje");

        _lines.Add(JournalLine.Create(accountId, debit, credit, note));
    }

    public void Post(string userId, string hash, string? previousHash)
    {
        if (Status != JournalStatus.Nacrt)
            throw new DomainException($"Temeljnica {Number} je već proknjižena");
        if (!_lines.Any())
            throw new DomainException("Temeljnica nema stavki");
        if (!IsBalanced)
            throw new DomainException(
                $"Temeljnica nije uravnotežena: D={TotalDebit} P={TotalCredit}");

        Status         = JournalStatus.Proknjiženo;
        PostedByUserId = userId;
        PostedAtUtc    = DateTime.UtcNow;
        IntegrityHash  = hash;
        PreviousHash   = previousHash;

        Raise(new JournalEntryPostedEvent(TenantId.Value, Id, Number, Date, TotalDebit));
    }

    public JournalEntry CreateReversal(string number)
    {
        if (Status != JournalStatus.Proknjiženo)
            throw new DomainException("Može se stornirati samo proknjižena temeljnica");

        var reversal = Create(TenantId, number, DateOnly.FromDateTime(DateTime.Today),
            $"Storno: {Description}");

        foreach (var line in _lines)
            reversal.AddLine(line.AccountId, line.Credit, line.Debit, $"Storno: {line.Note}");

        Status = JournalStatus.Stornirano;
        Raise(new JournalEntryReversedEvent(TenantId.Value, Id, reversal.Number));
        return reversal;
    }
}
