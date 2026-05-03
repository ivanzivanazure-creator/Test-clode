namespace AccountingERP.Infrastructure.Persistence.Configurations;

using AccountingERP.Domain.Aggregates.Invoice;
using AccountingERP.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.Description)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.Quantity)
               .HasColumnType("decimal(18,4)")
               .IsRequired();

        builder.Property(x => x.Unit)
               .HasMaxLength(20)
               .IsRequired();

        // UnitPrice as owned Money (Amount + Currency)
        builder.OwnsOne(x => x.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("UnitPriceAmount")
                 .HasColumnType("decimal(18,4)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("UnitPriceCurrency")
                 .HasColumnType("char(3)")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        // VatRate stored as decimal column
        builder.Property(x => x.VatRate)
               .HasColumnName("VatRatePercent")
               .HasColumnType("decimal(5,2)")
               .HasConversion(
                   v => v.Percent,
                   v => new VatRate(v))
               .IsRequired();

        // Computed properties ignored
        builder.Ignore(x => x.Total);
        builder.Ignore(x => x.VatAmount);
        builder.Ignore(x => x.TotalWithVat);
    }
}
