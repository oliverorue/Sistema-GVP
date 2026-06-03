using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Email)
            .HasMaxLength(200);

        builder.HasIndex(e => new { e.CompanyId, e.IsActive, e.Username })
            .HasDatabaseName("IX_Users_CompanyId_IsActive_Username");

        builder.HasIndex(e => new { e.CompanyId, e.Username })
            .IsUnique()
            .HasDatabaseName("IX_Users_CompanyId_Username");

        // Relaciones
        builder.HasOne(e => e.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
