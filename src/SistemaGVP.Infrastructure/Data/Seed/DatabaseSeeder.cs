using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Enums;
using SistemaGVP.Infrastructure.Services;

namespace SistemaGVP.Infrastructure.Data.Seed;

/// <summary>
/// Poblador inicial de la base de datos con datos demo para la Fase 1.
/// Crea: compañía demo, usuario admin, categorías, productos de ejemplo.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger? logger = null)
    {
        // Aplicar migraciones pendientes
        await context.Database.MigrateAsync();

        // Verificar si ya hay datos
        if (await context.Companies.AnyAsync())
        {
            logger?.LogInformation("Seed: Data already exists, skipping seed.");
            return;
        }

        logger?.LogInformation("Seed: No data found, starting seed...");

        // Deshabilitar auditoría durante el seed para evitar conflictos de FK
        // (los AuditLog se crearían con CompanyId=0 antes de que se genere el Id real)
        context.SuppressAudit = true;

        var passwordHasher = new PasswordHasherService();

        // ==========================================
        // 1. Compañía Demo
        // ==========================================
        var company = new Company
        {
            Name = "Mi Empresa S.A.",
            TaxId = "12345678-5",
            Address = "Av. Principal 1234",
            Phone = "+595 21 123 456",
            Email = "info@miempresa.com",
            TaxRate = 0.10m,
            Currency = "Gs.",
            LowStockThreshold = 10,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await context.Companies.AddAsync(company);
        await context.SaveChangesAsync();

        // ==========================================
        // 2. Usuario Administrador
        // ==========================================
        var adminUser = new User
        {
            CompanyId = company.Id,
            Username = "admin",
            FullName = "Administrador del Sistema",
            PasswordHash = passwordHasher.Hash("admin123"),
            Email = "admin@miempresa.com",
            Role = UserRole.Admin,
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var cashierUser = new User
        {
            CompanyId = company.Id,
            Username = "cajero1",
            FullName = "Cajero Principal",
            PasswordHash = passwordHasher.Hash("cajero123"),
            Email = "cajero1@miempresa.com",
            Role = UserRole.Cashier,
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await context.Users.AddRangeAsync(adminUser, cashierUser);
        await context.SaveChangesAsync();

        // ==========================================
        // 3. Categorías
        // ==========================================
        var categories = new List<Category>
        {
            new() { CompanyId = company.Id, Name = "Bebidas", Description = "Bebidas en general", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Alimentos", Description = "Alimentos en general", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Lácteos", Description = "Productos lácteos", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Limpieza", Description = "Artículos de limpieza", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Electrónicos", Description = "Artículos electrónicos y accesorios", CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // ==========================================
        // 4. Proveedores
        // ==========================================
        var suppliers = new List<Supplier>
        {
            new() { CompanyId = company.Id, Name = "Distribuidora XYZ", ContactName = "Juan Pérez", Phone = "+595 981 123 456", Email = "juan@xyz.com", TaxId = "87654321-0", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Mayorista ABC", ContactName = "María López", Phone = "+595 982 654 321", Email = "maria@abc.com", TaxId = "11223344-5", CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        await context.Suppliers.AddRangeAsync(suppliers);
        await context.SaveChangesAsync();

        // ==========================================
        // 5. Clientes
        // ==========================================
        var customers = new List<Customer>
        {
            new() { CompanyId = company.Id, Name = "Consumidor Final", TaxId = "00000000-0", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Carlos González", Phone = "+595 983 111 222", Email = "carlos@email.com", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Ana Martínez", Phone = "+595 984 333 444", CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        await context.Customers.AddRangeAsync(customers);
        await context.SaveChangesAsync();

        // ==========================================
        // 6. Productos
        // ==========================================
        var bebidasId = categories[0].Id;
        var alimentosId = categories[1].Id;
        var lacteosId = categories[2].Id;
        var limpiezaId = categories[3].Id;
        var electronicosId = categories[4].Id;
        var proveedor1Id = suppliers[0].Id;
        var proveedor2Id = suppliers[1].Id;

        var products = new List<Product>
        {
            // Bebidas
            new() { CompanyId = company.Id, CategoryId = bebidasId, SupplierId = proveedor1Id, Name = "Coca-Cola 500ml", Barcode = "7801234560001", Sku = "BEB-001", Price = 5000m, Cost = 3500m, MinStock = 10, CurrentStock = 50, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = bebidasId, SupplierId = proveedor1Id, Name = "Sprite 500ml", Barcode = "7801234560002", Sku = "BEB-002", Price = 5000m, Cost = 3500m, MinStock = 10, CurrentStock = 40, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = bebidasId, SupplierId = proveedor1Id, Name = "Agua Mineral 1L", Barcode = "7801234560003", Sku = "BEB-003", Price = 3000m, Cost = 2000m, MinStock = 20, CurrentStock = 30, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = bebidasId, SupplierId = proveedor1Id, Name = "Jugo de Naranja 1L", Barcode = "7801234560004", Sku = "BEB-004", Price = 8000m, Cost = 5500m, MinStock = 5, CurrentStock = 15, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },

            // Alimentos
            new() { CompanyId = company.Id, CategoryId = alimentosId, SupplierId = proveedor2Id, Name = "Arroz 1kg", Barcode = "7801234560005", Sku = "ALI-001", Price = 7000m, Cost = 5000m, MinStock = 20, CurrentStock = 100, Unit = "kg", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = alimentosId, SupplierId = proveedor2Id, Name = "Fideos Tallarín 500g", Barcode = "7801234560006", Sku = "ALI-002", Price = 4000m, Cost = 2800m, MinStock = 15, CurrentStock = 60, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = alimentosId, SupplierId = proveedor2Id, Name = "Aceite de Girasol 1L", Barcode = "7801234560007", Sku = "ALI-003", Price = 10000m, Cost = 7500m, MinStock = 10, CurrentStock = 25, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = alimentosId, SupplierId = proveedor2Id, Name = "Azúcar 1kg", Barcode = "7801234560008", Sku = "ALI-004", Price = 6000m, Cost = 4200m, MinStock = 15, CurrentStock = 45, Unit = "kg", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = alimentosId, SupplierId = proveedor2Id, Name = "Harina de Trigo 1kg", Barcode = "7801234560009", Sku = "ALI-005", Price = 5000m, Cost = 3500m, MinStock = 10, CurrentStock = 35, Unit = "kg", CreatedAt = DateTime.UtcNow, IsActive = true },

            // Lácteos
            new() { CompanyId = company.Id, CategoryId = lacteosId, SupplierId = proveedor1Id, Name = "Leche Entera 1L", Barcode = "7801234560010", Sku = "LAC-001", Price = 6000m, Cost = 4200m, MinStock = 20, CurrentStock = 40, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = lacteosId, SupplierId = proveedor1Id, Name = "Yogurt Natural 200g", Barcode = "7801234560011", Sku = "LAC-002", Price = 4000m, Cost = 2800m, MinStock = 10, CurrentStock = 20, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = lacteosId, SupplierId = proveedor1Id, Name = "Queso Paraguay 500g", Barcode = "7801234560012", Sku = "LAC-003", Price = 15000m, Cost = 11000m, MinStock = 5, CurrentStock = 12, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },

            // Limpieza
            new() { CompanyId = company.Id, CategoryId = limpiezaId, SupplierId = proveedor2Id, Name = "Detergente Líquido 500ml", Barcode = "7801234560013", Sku = "LIM-001", Price = 8000m, Cost = 5600m, MinStock = 10, CurrentStock = 25, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = limpiezaId, SupplierId = proveedor2Id, Name = "Lavandina 1L", Barcode = "7801234560014", Sku = "LIM-002", Price = 5000m, Cost = 3500m, MinStock = 10, CurrentStock = 30, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = limpiezaId, SupplierId = proveedor2Id, Name = "Esponja Multiuso", Barcode = "7801234560015", Sku = "LIM-003", Price = 3000m, Cost = 1800m, MinStock = 20, CurrentStock = 50, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },

            // Electrónicos
            new() { CompanyId = company.Id, CategoryId = electronicosId, SupplierId = proveedor1Id, Name = "Pilas AA (4 unidades)", Barcode = "7801234560016", Sku = "ELE-001", Price = 10000m, Cost = 7000m, MinStock = 10, CurrentStock = 20, Unit = "pza", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = electronicosId, SupplierId = proveedor1Id, Name = "Cable USB-C", Barcode = "7801234560017", Sku = "ELE-002", Price = 25000m, Cost = 15000m, MinStock = 5, CurrentStock = 15, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = electronicosId, SupplierId = proveedor2Id, Name = "Auriculares Bluetooth", Barcode = "7801234560018", Sku = "ELE-003", Price = 80000m, Cost = 55000m, MinStock = 3, CurrentStock = 8, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // ==========================================
        // 7. Inventario Inicial (Entrada de stock inicial)
        // ==========================================
        var inventoryMovements = products.Select(p => new InventoryMovement
        {
            ProductId = p.Id,
            UserId = adminUser.Id,
            CompanyId = company.Id,
            Type = MovementType.IN,
            Quantity = p.CurrentStock,
            StockBefore = 0,
            StockAfter = p.CurrentStock,
            Reason = "Stock inicial",
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await context.InventoryMovements.AddRangeAsync(inventoryMovements);
        await context.SaveChangesAsync();

        logger?.LogInformation("🔍 SEED: Seed completed successfully. Created: 1 company, 2 users, {CatCount} categories, {SupCount} suppliers, {CustCount} customers, {ProdCount} products, {MovCount} inventory movements",
            categories.Count, suppliers.Count, customers.Count, products.Count, inventoryMovements.Count);
    }
}
