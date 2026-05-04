namespace AccountingERP.Infrastructure.Persistence.Configurations;

using AccountingERP.Domain.Aggregates.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.TenantId)
               .IsRequired();

        builder.Property(x => x.Name)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.PIB)
               .HasColumnType("char(9)")
               .HasMaxLength(9)
               .IsRequired(false);

        builder.Property(x => x.MaticniBroj)
               .HasColumnType("char(8)")
               .HasMaxLength(8)
               .IsRequired(false);

        builder.Property(x => x.Address)
               .HasMaxLength(300)
               .IsRequired(false);

        builder.Property(x => x.City)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.Country)
               .HasMaxLength(2)
               .HasDefaultValue("RS")
               .IsRequired();

        builder.Property(x => x.IBAN)
               .HasMaxLength(34)
               .IsRequired(false);

        builder.Property(x => x.Email)
               .HasMaxLength(200)
               .IsRequired(false);

        builder.Property(x => x.Phone)
               .HasMaxLength(50)
               .IsRequired(false);

        builder.Property(x => x.IsActive)
               .IsRequired();

        builder.Property(x => x.ClientType)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.PIB })
               .HasDatabaseName("IX_Clients_TenantId_PIB");
    }
}
