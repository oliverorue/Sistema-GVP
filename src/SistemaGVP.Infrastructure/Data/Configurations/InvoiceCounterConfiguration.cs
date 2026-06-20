using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Infrastructure.Data.Configurations;

public class InvoiceCounterConfiguration : IEntityTypeConfiguration<InvoiceCounter>
{
    public void Configure(EntityTypeBuilder<InvoiceCounter> builder)
    {
        builder.ToTable("InvoiceCounters");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DatePrefix)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(e => e.LastNumber)
            .IsRequired()
            .HasDefaultValue(0);

        // Índice único: una compañía solo tiene un contador por fecha
        builder.HasIndex(e => new { e.CompanyId, e.DatePrefix })
            .IsUnique();

        builder.HasOne(e => e.Company)
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
