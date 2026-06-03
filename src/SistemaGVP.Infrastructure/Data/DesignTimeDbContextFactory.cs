using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;

namespace SistemaGVP.Infrastructure.Data;

/// <summary>
/// Fábrica para migraciones en tiempo de diseño (dotnet ef migrations add, dotnet ef database update).
/// Usa el mismo directorio de base de datos que la aplicación en runtime:
/// ~/.local/share/SistemaGVP/sistemagvp.db
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var dbDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SistemaGVP");

        Directory.CreateDirectory(dbDirectory);
        var dbPath = Path.Combine(dbDirectory, "sistemagvp.db");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new AppDbContext(optionsBuilder.Options);
    }
}
