namespace AccountingERP.Infrastructure.Persistence;

using System.Reflection;
using AccountingERP.Domain.Aggregates.AccountingPeriod;
using AccountingERP.Domain.Aggregates.Client;
using AccountingERP.Domain.Aggregates.Employee;
using AccountingERP.Domain.Aggregates.Invoice;
using AccountingERP.Domain.Aggregates.JournalEntry;
using AccountingERP.Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Invoice>           Invoices           => Set<Invoice>();
    public DbSet<InvoiceItem>       InvoiceItems       => Set<InvoiceItem>();
    public DbSet<JournalEntry>      JournalEntries     => Set<JournalEntry>();
    public DbSet<JournalLine>       JournalLines       => Set<JournalLine>();
    public DbSet<Employee>          Employees          => Set<Employee>();
    public DbSet<Client>            Clients            => Set<Client>();
    public DbSet<AccountingPeriod>  AccountingPeriods  => Set<AccountingPeriod>();
    public DbSet<User>              Users              => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Metadata.FindProperty("CreatedAt") is not null)
                    entry.Property("CreatedAt").CurrentValue = now;

                if (entry.Metadata.FindProperty("UpdatedAt") is not null)
                    entry.Property("UpdatedAt").CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Metadata.FindProperty("UpdatedAt") is not null)
                    entry.Property("UpdatedAt").CurrentValue = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
