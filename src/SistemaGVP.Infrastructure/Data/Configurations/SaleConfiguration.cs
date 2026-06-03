using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Infrastructure.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Índice único: InvoiceNumber único por compañía
        builder.HasIndex(e => new { e.CompanyId, e.InvoiceNumber })
            .IsUnique()
            .HasDatabaseName("IX_Sales_CompanyId_InvoiceNumber");

        // Índice para búsquedas por fecha
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_Sales_CreatedAt");

        builder.HasIndex(e => new { e.CompanyId, e.CreatedAt })
            .HasDatabaseName("IX_Sales_CompanyId_CreatedAt");

        // Relaciones
        builder.HasOne(e => e.Company)
            .WithMany(c => c.Sales)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany(u => u.Sales)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
