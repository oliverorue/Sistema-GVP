using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Barcode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.ImagePath)
            .HasMaxLength(500);

        builder.HasIndex(e => new { e.CompanyId, e.IsActive, e.Name })
            .HasDatabaseName("IX_Products_CompanyId_IsActive_Name");

        builder.HasIndex(e => new { e.CompanyId, e.Barcode })
            .IsUnique()
            .HasDatabaseName("IX_Products_CompanyId_Barcode");

        builder.HasIndex(e => new { e.CompanyId, e.Sku })
            .IsUnique()
            .HasDatabaseName("IX_Products_CompanyId_Sku");

        // Relaciones
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Company)
            .WithMany(c => c.Products)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
