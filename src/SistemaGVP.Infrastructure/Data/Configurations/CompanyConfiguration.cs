using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.TaxId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.Phone)
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasMaxLength(200);

        builder.Property(e => e.Logo)
            .HasMaxLength(500);

        builder.Property(e => e.LogoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.TaxRate)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.IvaIncluido)
            .HasDefaultValue(true);

        builder.Property(e => e.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("Gs.");

        builder.Property(e => e.LowStockThreshold)
            .HasDefaultValue(10);
    }
}
