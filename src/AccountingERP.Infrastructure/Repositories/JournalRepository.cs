namespace AccountingERP.Infrastructure.Repositories;

using AccountingERP.Application.Queries.Journal;
using AccountingERP.Domain.Aggregates.JournalEntry;
using AccountingERP.Domain.Exceptions;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Domain.ValueObjects;
using AccountingERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class JournalRepository : IJournalRepository, IJournalEntriesReader
{
    private readonly AppDbContext _db;

    public JournalRepository(AppDbContext db) => _db = db;

    // ── IRepository<JournalEntry, int> ────────────────────────────────────────

    public async Task<JournalEntry?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.JournalEntries
                    .Include(e => e.Lines)
                    .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<JournalEntry> GetByIdOrThrowAsync(int id, CancellationToken ct = default)
        => await GetByIdAsync(id, ct)
           ?? throw new NotFoundException(nameof(JournalEntry), id);

    public void Add(JournalEntry entity)    => _db.JournalEntries.Add(entity);
    public void Remove(JournalEntry entity) => _db.JournalEntries.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    // ── IJournalRepository ────────────────────────────────────────────────────

    /// <summary>
    /// Returns the IntegrityHash of the most recently posted journal entry for the tenant,
    /// ordered by Date descending then Id descending to form the hash chain.
    /// Returns null when there are no entries yet (genesis block).
    /// </summary>
    public async Task<string?> GetLastHashAsync(int tenantId, CancellationToken ct)
    {
        var tid = new TenantId(tenantId);

        return await _db.JournalEntries
                        .AsNoTracking()
                        .Where(e => e.TenantId == tid
                                 && e.Status   == JournalStatus.Proknjiženo
                                 && e.IntegrityHash != null)
                        .OrderByDescending(e => e.Date)
                        .ThenByDescending(e => e.Id)
                        .Select(e => e.IntegrityHash)
                        .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Checks whether the given accounting period (month/year) is locked for the tenant.
    /// The check queries the AccountingPeriods shadow table; if the table or row does not
    /// exist the period is considered open (returns false).
    /// </summary>
    public async Task<bool> IsPeriodLockedAsync(
        int tenantId, int month, int year, CancellationToken ct)
    {
        // Use raw SQL so we can query a table that is not part of the main model.
        // If the AccountingPeriods table does not exist the query returns no rows, which
        // means the period is unlocked — safe default.
        try
        {
            var sql = $"""
                SELECT COUNT(1)
                FROM   AccountingPeriods
                WHERE  TenantId = {tenantId}
                  AND  [Month]  = {month}
                  AND  [Year]   = {year}
                  AND  IsLocked = 1
                """;

#pragma warning disable EF1002  // Raw SQL — intentional; table is not in the EF model
            var count = await _db.Database
                                 .SqlQueryRaw<int>(sql)
                                 .FirstOrDefaultAsync(ct);
#pragma warning restore EF1002

            return count > 0;
        }
        catch (Exception)
        {
            // Table does not exist or DB is unreachable — treat period as open.
            return false;
        }
    }

    /// <summary>
    /// Returns the account balance sheet as of <paramref name="asOf"/> for the tenant.
    /// Joins JournalLines with the ChartOfAccounts shadow table to retrieve account metadata,
    /// then aggregates Debit minus Credit per account.
    /// </summary>
    public async Task<IEnumerable<AccountBalance>> GetBalancesAsync(
        int tenantId, DateOnly asOf, CancellationToken ct)
    {
        var tid        = new TenantId(tenantId);
        var asOfDate   = asOf.ToDateTime(TimeOnly.MinValue);

        // Load posted lines for the tenant up to asOf, grouped by AccountId.
        var lineGroups = await _db.JournalEntries
                                  .AsNoTracking()
                                  .Where(e => e.TenantId == tid
                                           && e.Status   == JournalStatus.Proknjiženo
                                           && e.Date     <= asOf)
                                  .SelectMany(e => e.Lines)
                                  .GroupBy(l => l.AccountId)
                                  .Select(g => new
                                  {
                                      AccountId    = g.Key,
                                      TotalDebit   = g.Sum(l => l.Debit.Amount),
                                      TotalCredit  = g.Sum(l => l.Credit.Amount),
                                  })
                                  .ToListAsync(ct);

        // Attempt to join with ChartOfAccounts for name/type metadata via raw SQL.
        // Falls back to numeric code if ChartOfAccounts table does not exist.
        var accountIds  = lineGroups.Select(g => g.AccountId).ToList();
        var accountMeta = new Dictionary<int, (string Code, string Name, string Type)>();

        if (accountIds.Count > 0)
        {
            try
            {
                var idList = string.Join(",", accountIds);
                var sql    = $"""
                    SELECT Id, Code, [Name], AccountType
                    FROM   ChartOfAccounts
                    WHERE  Id IN ({idList})
                    """;

                // Project into a temporary DTO to avoid coupling to shadow model.
                var rows = await _db.Database
                                    .SqlQueryRaw<ChartOfAccountRow>(sql)
                                    .ToListAsync(ct);

                foreach (var row in rows)
                    accountMeta[row.Id] = (row.Code, row.Name, row.AccountType);
            }
            catch (Exception)
            {
                // ChartOfAccounts table not present — degrade gracefully.
            }
        }

        var balances = lineGroups.Select(g =>
        {
            var hasMetadata = accountMeta.TryGetValue(g.AccountId, out var meta);
            var code        = hasMetadata ? meta.Code : g.AccountId.ToString();
            var name        = hasMetadata ? meta.Name : $"Account {g.AccountId}";
            var type        = hasMetadata ? meta.Type : "Unknown";
            var balance     = g.TotalDebit - g.TotalCredit;

            return new AccountBalance(
                Code:    code,
                Name:    name,
                Type:    type,
                Balance: Money.FromRSD(balance));
        })
        .OrderBy(b => b.Code)
        .ToList();

        return balances;
    }

    // ── IJournalEntriesReader ─────────────────────────────────────────────────

    /// <summary>
    /// Returns a page of journal entries matching the given filter, together with
    /// the total count for pagination metadata.
    /// </summary>
    public async Task<(IEnumerable<JournalEntry> Entries, int TotalCount)> GetPagedAsync(
        int               tenantId,
        JournalFilter     filter,
        CancellationToken ct = default)
    {
        var tid   = new TenantId(tenantId);
        var query = _db.JournalEntries
                       .Include(e => e.Lines)
                       .AsNoTracking()
                       .Where(e => e.TenantId == tid);

        if (!string.IsNullOrWhiteSpace(filter.Status)
            && Enum.TryParse<JournalStatus>(filter.Status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(e => e.Status == parsedStatus);
        }

        if (filter.DateFrom.HasValue)
            query = query.Where(e => e.Date >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(e => e.Date <= filter.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.SourceType))
            query = query.Where(e => e.SourceType == filter.SourceType);

        var totalCount = await query.CountAsync(ct);

        var page     = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        var entries = await query
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (entries, totalCount);
    }

    // ── Private helper projection ──────────────────────────────────────────────

    private sealed record ChartOfAccountRow(int Id, string Code, string Name, string AccountType);
}
