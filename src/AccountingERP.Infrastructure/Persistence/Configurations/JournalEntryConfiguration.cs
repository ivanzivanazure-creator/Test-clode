namespace AccountingERP.Infrastructure.Persistence.Configurations;

using AccountingERP.Domain.Aggregates.JournalEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        // TenantId stored as int column
        builder.Property(x => x.TenantId)
               .HasColumnName("TenantId")
               .HasConversion(
                   tenantId => tenantId.Value,
                   value    => new Domain.ValueObjects.TenantId(value))
               .IsRequired();

        builder.Property(x => x.Number)
               .HasMaxLength(50)
               .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Number })
               .IsUnique()
               .HasDatabaseName("IX_JournalEntries_TenantId_Number");

        builder.Property(x => x.Date)
               .HasColumnType("date")
               .IsRequired();

        builder.Property(x => x.Description)
               .HasMaxLength(500)
               .IsRequired();

        // Status stored as string
        builder.Property(x => x.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.SourceType)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.SourceId)
               .IsRequired(false);

        builder.Property(x => x.PostedByUserId)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.PostedAtUtc)
               .IsRequired(false);

        builder.Property(x => x.IntegrityHash)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.PreviousHash)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.HasMany(x => x.Lines)
               .WithOwner()
               .HasForeignKey("JournalEntryId")
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines)
               .UsePropertyAccessMode(PropertyAccessMode.Field)
               .HasField("_lines");

        // Computed properties ignored
        builder.Ignore(x => x.TotalDebit);
        builder.Ignore(x => x.TotalCredit);
        builder.Ignore(x => x.IsBalanced);
        builder.Ignore(x => x.RetainUntil);
    }
}
