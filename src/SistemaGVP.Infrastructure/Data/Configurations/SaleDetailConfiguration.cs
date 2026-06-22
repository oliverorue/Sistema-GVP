using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Infrastructure.Data.Configurations;

public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ProductName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Barcode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Unit)
            .HasMaxLength(20)
            .HasDefaultValue("pz");

        // Relaciones
        builder.HasOne(e => e.Sale)
            .WithMany(s => s.SaleDetails)
            .HasForeignKey(e => e.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
            .WithMany(p => p.SaleDetails)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
