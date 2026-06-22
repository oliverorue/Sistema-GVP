using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Enums;
using SistemaGVP.Infrastructure.Services;

namespace SistemaGVP.Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger? logger = null)
    {
        await context.Database.MigrateAsync();

        if (await context.Companies.AnyAsync())
        {
            logger?.LogInformation("Seed: Data already exists, skipping seed.");
            return;
        }

        logger?.LogInformation("Seed: No data found, starting seed...");

        context.SuppressAudit = true;

        var passwordHasher = new PasswordHasherService();

        var company = new Company
        {
            Name = "Ferretería Paraguaya S.A.",
            TaxId = "80012345-6",
            Address = "Av. Mcal. López 4567, Asunción",
            Phone = "+595 21 500 123",
            Email = "info@ferreteriapy.com",
            TaxRate = 0.10m,
            IvaIncluido = true,
            Currency = "Gs.",
            LowStockThreshold = 10,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await context.Companies.AddAsync(company);
        await context.SaveChangesAsync();

        var adminUser = new User
        {
            CompanyId = company.Id,
            Username = "admin",
            FullName = "Administrador del Sistema",
            PasswordHash = passwordHasher.Hash("admin123"),
            Email = "admin@ferreteriapy.com",
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
            Email = "cajero1@ferreteriapy.com",
            Role = UserRole.Cashier,
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await context.Users.AddRangeAsync(adminUser, cashierUser);
        await context.SaveChangesAsync();

        var categories = new List<Category>
        {
            new() { CompanyId = company.Id, Name = "Fijaciones", Description = "Clavos, tornillos, tarugos", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Materiales", Description = "Cemento, arena, cal", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Electricidad", Description = "Cables, enchufes, cintas", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Pinturas", Description = "Látex, esmalte, pinceles", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Caños y Tubos", Description = "Caños, codos, tees PVC", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Ferretería General", Description = "Candados, bisagras, lijas", CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var suppliers = new List<Supplier>
        {
            new() { CompanyId = company.Id, Name = "Ferremix S.A.", ContactName = "Carlos López", Phone = "+595 981 111 222", Email = "ventas@ferremix.com", TaxId = "87654321-0", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Distribuidora Paraguay S.R.L.", ContactName = "Laura Gómez", Phone = "+595 982 333 444", Email = "info@distribuidorapy.com", TaxId = "11223344-5", CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        await context.Suppliers.AddRangeAsync(suppliers);
        await context.SaveChangesAsync();

        var customers = new List<Customer>
        {
            new() { CompanyId = company.Id, Name = "Consumidor Final", TaxId = "00000000-0", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Constructor Obras S.A.", Phone = "+595 983 555 666", Email = "obras@constructor.com", CreditLimit = 5000000m, Balance = 0, CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, Name = "Arq. Martínez", Phone = "+595 984 777 888", CreditLimit = 2000000m, Balance = 500000m, CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        await context.Customers.AddRangeAsync(customers);
        await context.SaveChangesAsync();

        var fijacionesId = categories[0].Id;
        var materialesId = categories[1].Id;
        var electricidadId = categories[2].Id;
        var pinturasId = categories[3].Id;
        var caniosId = categories[4].Id;
        var ferreteriaId = categories[5].Id;
        var proveedor1Id = suppliers[0].Id;
        var proveedor2Id = suppliers[1].Id;

        var products = new List<Product>
        {
            // Fijaciones
            new() { CompanyId = company.Id, CategoryId = fijacionesId, SupplierId = proveedor1Id, Name = "Clavo 2 pulgadas", Barcode = "FIJ-001", Sku = "FIJ-001", Price = 12000m, Cost = 8500m, MinStock = 10, CurrentStock = 25, Unit = "kg", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = fijacionesId, SupplierId = proveedor1Id, Name = "Tornillo 8mm x 50mm", Barcode = "FIJ-002", Sku = "FIJ-002", Price = 150m, Cost = 90m, MinStock = 100, CurrentStock = 500, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = fijacionesId, SupplierId = proveedor1Id, Name = "Tarugo plástico 6mm", Barcode = "FIJ-003", Sku = "FIJ-003", Price = 100m, Cost = 50m, MinStock = 200, CurrentStock = 1000, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },

            // Materiales
            new() { CompanyId = company.Id, CategoryId = materialesId, SupplierId = proveedor2Id, Name = "Cemento CPC-40", Barcode = "MAT-001", Sku = "MAT-001", Price = 55000m, Cost = 42000m, MinStock = 20, CurrentStock = 100, Unit = "bolsa", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = materialesId, SupplierId = proveedor2Id, Name = "Arena lavada", Barcode = "MAT-002", Sku = "MAT-002", Price = 85000m, Cost = 55000m, MinStock = 10, CurrentStock = 50, Unit = "m³", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = materialesId, SupplierId = proveedor2Id, Name = "Cal hidratada 20kg", Barcode = "MAT-003", Sku = "MAT-003", Price = 25000m, Cost = 18000m, MinStock = 15, CurrentStock = 60, Unit = "bolsa", CreatedAt = DateTime.UtcNow, IsActive = true },

            // Electricidad
            new() { CompanyId = company.Id, CategoryId = electricidadId, SupplierId = proveedor1Id, Name = "Cable 2.5mm", Barcode = "ELE-001", Sku = "ELE-001", Price = 3500m, Cost = 2200m, MinStock = 50, CurrentStock = 200, Unit = "m", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = electricidadId, SupplierId = proveedor1Id, Name = "Enchufe bipolar 10A", Barcode = "ELE-002", Sku = "ELE-002", Price = 8500m, Cost = 5500m, MinStock = 30, CurrentStock = 150, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = electricidadId, SupplierId = proveedor1Id, Name = "Cinta aisladora", Barcode = "ELE-003", Sku = "ELE-003", Price = 5000m, Cost = 2800m, MinStock = 20, CurrentStock = 80, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },

            // Pinturas
            new() { CompanyId = company.Id, CategoryId = pinturasId, SupplierId = proveedor2Id, Name = "Látex interior blanco 20L", Barcode = "PIN-001", Sku = "PIN-001", Price = 180000m, Cost = 130000m, MinStock = 10, CurrentStock = 40, Unit = "pza", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = pinturasId, SupplierId = proveedor2Id, Name = "Esmalte sintético 1GL", Barcode = "PIN-002", Sku = "PIN-002", Price = 95000m, Cost = 68000m, MinStock = 8, CurrentStock = 30, Unit = "pza", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = pinturasId, SupplierId = proveedor2Id, Name = "Pincel 2 pulgadas", Barcode = "PIN-003", Sku = "PIN-003", Price = 15000m, Cost = 8500m, MinStock = 15, CurrentStock = 60, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },

            // Caños y Tubos
            new() { CompanyId = company.Id, CategoryId = caniosId, SupplierId = proveedor1Id, Name = "Caño PVC 1/2\"", Barcode = "CAÑ-001", Sku = "CAÑ-001", Price = 6500m, Cost = 4200m, MinStock = 30, CurrentStock = 100, Unit = "m", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = caniosId, SupplierId = proveedor1Id, Name = "Codo PVC 90° 1/2\"", Barcode = "CAÑ-002", Sku = "CAÑ-002", Price = 2500m, Cost = 1300m, MinStock = 40, CurrentStock = 200, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = caniosId, SupplierId = proveedor1Id, Name = "Tee PVC 1/2\"", Barcode = "CAÑ-003", Sku = "CAÑ-003", Price = 3500m, Cost = 1800m, MinStock = 30, CurrentStock = 150, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },

            // Ferretería General
            new() { CompanyId = company.Id, CategoryId = ferreteriaId, SupplierId = proveedor2Id, Name = "Candado 30mm", Barcode = "FER-001", Sku = "FER-001", Price = 22000m, Cost = 14000m, MinStock = 10, CurrentStock = 45, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = ferreteriaId, SupplierId = proveedor2Id, Name = "Bisagra 3\"", Barcode = "FER-002", Sku = "FER-002", Price = 4500m, Cost = 2500m, MinStock = 20, CurrentStock = 80, Unit = "par", CreatedAt = DateTime.UtcNow, IsActive = true },
            new() { CompanyId = company.Id, CategoryId = ferreteriaId, SupplierId = proveedor2Id, Name = "Lija N°100", Barcode = "FER-003", Sku = "FER-003", Price = 3500m, Cost = 1800m, MinStock = 30, CurrentStock = 120, Unit = "pz", CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

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

        logger?.LogInformation("Seed: Completed — 1 company, 2 users, {CatCount} categories, {SupCount} suppliers, {CustCount} customers, {ProdCount} products, {MovCount} inventory movements",
            categories.Count, suppliers.Count, customers.Count, products.Count, inventoryMovements.Count);
    }
}
