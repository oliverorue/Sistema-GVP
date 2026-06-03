using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.ContactName)
            .HasMaxLength(200);

        builder.Property(e => e.Phone)
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasMaxLength(200);

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.TaxId)
            .HasMaxLength(50);

        builder.HasIndex(e => new { e.CompanyId, e.IsActive, e.Name })
            .HasDatabaseName("IX_Suppliers_CompanyId_IsActive_Name");

        builder.HasIndex(e => new { e.CompanyId, e.Name })
            .IsUnique()
            .HasDatabaseName("IX_Suppliers_CompanyId_Name");

        // Relaciones
        builder.HasOne(e => e.Company)
            .WithMany(c => c.Suppliers)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
