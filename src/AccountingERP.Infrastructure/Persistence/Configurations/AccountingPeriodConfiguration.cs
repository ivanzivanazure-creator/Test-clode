namespace AccountingERP.Infrastructure.Persistence.Configurations;

using AccountingERP.Domain.Aggregates.AccountingPeriod;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AccountingPeriodConfiguration : IEntityTypeConfiguration<AccountingPeriod>
{
    public void Configure(EntityTypeBuilder<AccountingPeriod> builder)
    {
        builder.ToTable("AccountingPeriods");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.TenantId)
               .IsRequired();

        builder.Property(x => x.Month)
               .IsRequired();

        builder.Property(x => x.Year)
               .IsRequired();

        builder.Property(x => x.IsLocked)
               .IsRequired();

        builder.Property(x => x.LockedAt)
               .IsRequired(false);

        builder.Property(x => x.LockedByUserId)
               .HasMaxLength(100)
               .IsRequired(false);

        // One period per tenant per month/year
        builder.HasIndex(x => new { x.TenantId, x.Year, x.Month })
               .IsUnique()
               .HasDatabaseName("IX_AccountingPeriods_TenantId_Year_Month");
    }
}
