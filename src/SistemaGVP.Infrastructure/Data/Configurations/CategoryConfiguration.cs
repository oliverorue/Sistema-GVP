using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.HasIndex(e => new { e.CompanyId, e.IsActive, e.Name })
            .HasDatabaseName("IX_Categories_CompanyId_IsActive_Name");

        builder.HasIndex(e => new { e.CompanyId, e.Name })
            .IsUnique()
            .HasDatabaseName("IX_Categories_CompanyId_Name");

        // Relaciones
        builder.HasOne(e => e.Company)
            .WithMany(c => c.Categories)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
