namespace AccountingERP.Infrastructure.Persistence.Configurations;

using AccountingERP.Domain.Aggregates.JournalEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class JournalLineConfiguration : IEntityTypeConfiguration<JournalLine>
{
    public void Configure(EntityTypeBuilder<JournalLine> builder)
    {
        builder.ToTable("JournalLines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.AccountId)
               .IsRequired();

        // Debit as owned Money
        builder.OwnsOne(x => x.Debit, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("DebitAmount")
                 .HasColumnType("decimal(18,4)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("DebitCurrency")
                 .HasColumnType("char(3)")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        // Credit as owned Money
        builder.OwnsOne(x => x.Credit, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("CreditAmount")
                 .HasColumnType("decimal(18,4)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("CreditCurrency")
                 .HasColumnType("char(3)")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        builder.Property(x => x.Note)
               .HasMaxLength(500)
               .IsRequired(false);
    }
}
