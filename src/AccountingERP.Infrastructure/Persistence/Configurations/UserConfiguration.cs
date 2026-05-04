namespace AccountingERP.Infrastructure.Persistence.Configurations;

using AccountingERP.Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.TenantId)
               .IsRequired();

        builder.Property(x => x.Username)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.PasswordHash)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.Email)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.FullName)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Role)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.IsActive)
               .IsRequired();

        // Username unique per tenant
        builder.HasIndex(x => new { x.TenantId, x.Username })
               .IsUnique()
               .HasDatabaseName("IX_Users_TenantId_Username");
    }
}
