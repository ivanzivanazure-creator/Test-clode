namespace AccountingERP.Infrastructure.Persistence.Configurations;

using AccountingERP.Domain.Aggregates.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

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

        builder.Property(x => x.FirstName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.LastName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.Position)
               .HasMaxLength(200)
               .IsRequired();

        // GrossSalary as owned Money
        builder.OwnsOne(x => x.GrossSalary, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("GrossSalaryAmount")
                 .HasColumnType("decimal(18,4)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("GrossSalaryCurrency")
                 .HasColumnType("char(3)")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        builder.Property(x => x.HireDate)
               .HasColumnType("date")
               .IsRequired();

        builder.Property(x => x.TermDate)
               .HasColumnType("date")
               .IsRequired(false);

        // TaxExemption stored as string
        builder.Property(x => x.TaxExemption)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(x => x.IsActive)
               .IsRequired();

        // Encrypted sensitive data (ZZPL 87/2018)
        builder.Property(x => x.JMBGEncrypted)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(x => x.JMBGHashSha256)
               .HasMaxLength(64)
               .IsRequired(false);

        // Index on hash for lookup without decryption
        builder.HasIndex(x => new { x.TenantId, x.JMBGHashSha256 })
               .HasDatabaseName("IX_Employees_TenantId_JMBGHash");

        builder.Property(x => x.BankAccountEncrypted)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(x => x.EmailEncrypted)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(x => x.IsPseudonymized)
               .IsRequired();

        builder.Property(x => x.PseudonymizedAt)
               .IsRequired(false);

        // Computed property ignored
        builder.Ignore(x => x.FullName);
    }
}
