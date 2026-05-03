namespace AccountingERP.Infrastructure.Persistence.Configurations;

using AccountingERP.Domain.Aggregates.Invoice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

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

        // Unique invoice number per tenant
        builder.HasIndex(x => new { x.TenantId, x.Number })
               .IsUnique()
               .HasDatabaseName("IX_Invoices_TenantId_Number");

        builder.Property(x => x.ClientId)
               .IsRequired();

        builder.Property(x => x.IssueDate)
               .HasColumnType("date")
               .IsRequired();

        builder.Property(x => x.DueDate)
               .HasColumnType("date")
               .IsRequired();

        // Status stored as string
        builder.Property(x => x.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.IntegrityHash)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.HasMany(x => x.Items)
               .WithOwner()
               .HasForeignKey("InvoiceId")
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
               .UsePropertyAccessMode(PropertyAccessMode.Field)
               .HasField("_items");

        builder.Ignore(x => x.Amount);
        builder.Ignore(x => x.TaxAmount);
        builder.Ignore(x => x.TotalAmount);
        builder.Ignore(x => x.RetainUntil);
    }
}
