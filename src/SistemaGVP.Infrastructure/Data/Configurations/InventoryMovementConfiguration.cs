using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Infrastructure.Data.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Índices para consultas frecuentes
        builder.HasIndex(e => new { e.ProductId, e.CreatedAt })
            .HasDatabaseName("IX_InventoryMovements_ProductId_CreatedAt");

        builder.HasIndex(e => new { e.CompanyId, e.CreatedAt })
            .HasDatabaseName("IX_InventoryMovements_CompanyId_CreatedAt");

        builder.HasIndex(e => e.Type)
            .HasDatabaseName("IX_InventoryMovements_Type");

        // Relaciones
        builder.HasOne(e => e.Product)
            .WithMany(p => p.InventoryMovements)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany(u => u.InventoryMovements)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Company)
            .WithMany(c => c.InventoryMovements)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.RelatedSale)
            .WithMany(s => s.InventoryMovements)
            .HasForeignKey(e => e.RelatedSaleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
