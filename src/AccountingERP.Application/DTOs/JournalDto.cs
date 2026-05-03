namespace AccountingERP.Application.DTOs;

using AccountingERP.Domain.Aggregates.JournalEntry;

/// <summary>List-view DTO for a journal entry (temeljnica).</summary>
public sealed record JournalEntryDto(
    int      Id,
    int      TenantId,
    string   Number,
    DateOnly Date,
    string   Description,
    string   Status,
    decimal  TotalDebit,
    decimal  TotalCredit,
    bool     IsBalanced,
    string?  SourceType,
    int?     SourceId)
{
    public static JournalEntryDto FromDomain(JournalEntry entry) => new(
        Id:          entry.Id,
        TenantId:    entry.TenantId.Value,
        Number:      entry.Number,
        Date:        entry.Date,
        Description: entry.Description,
        Status:      entry.Status.ToString(),
        TotalDebit:  entry.TotalDebit.Amount,
        TotalCredit: entry.TotalCredit.Amount,
        IsBalanced:  entry.IsBalanced,
        SourceType:  entry.SourceType,
        SourceId:    entry.SourceId);
}

/// <summary>DTO for a single journal line (stavka temeljnice).</summary>
public sealed record JournalLineDto(
    int     AccountId,
    decimal Debit,
    decimal Credit,
    string? Note)
{
    public static JournalLineDto FromDomain(JournalLine line) => new(
        AccountId: line.AccountId,
        Debit:     line.Debit.Amount,
        Credit:    line.Credit.Amount,
        Note:      line.Note);
}

/// <summary>Detailed DTO that includes the individual journal lines.</summary>
public sealed record JournalEntryDetailDto(
    int                         Id,
    int                         TenantId,
    string                      Number,
    DateOnly                    Date,
    string                      Description,
    string                      Status,
    decimal                     TotalDebit,
    decimal                     TotalCredit,
    bool                        IsBalanced,
    string?                     SourceType,
    int?                        SourceId,
    string?                     PostedByUserId,
    DateTime?                   PostedAtUtc,
    IReadOnlyList<JournalLineDto> Lines)
{
    public static JournalEntryDetailDto FromDomain(JournalEntry entry) => new(
        Id:            entry.Id,
        TenantId:      entry.TenantId.Value,
        Number:        entry.Number,
        Date:          entry.Date,
        Description:   entry.Description,
        Status:        entry.Status.ToString(),
        TotalDebit:    entry.TotalDebit.Amount,
        TotalCredit:   entry.TotalCredit.Amount,
        IsBalanced:    entry.IsBalanced,
        SourceType:    entry.SourceType,
        SourceId:      entry.SourceId,
        PostedByUserId: entry.PostedByUserId,
        PostedAtUtc:   entry.PostedAtUtc,
        Lines:         entry.Lines.Select(JournalLineDto.FromDomain).ToList().AsReadOnly());
}
