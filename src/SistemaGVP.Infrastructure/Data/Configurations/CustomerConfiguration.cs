using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.TaxId)
            .HasMaxLength(50);

        builder.Property(e => e.Phone)
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasMaxLength(200);

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.HasIndex(e => new { e.CompanyId, e.IsActive, e.Name })
            .HasDatabaseName("IX_Customers_CompanyId_IsActive_Name");

        builder.HasIndex(e => new { e.CompanyId, e.TaxId })
            .IsUnique()
            .HasDatabaseName("IX_Customers_CompanyId_TaxId")
            .HasFilter("[TaxId] IS NOT NULL");

        // Relaciones
        builder.HasOne(e => e.Company)
            .WithMany(c => c.Customers)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
