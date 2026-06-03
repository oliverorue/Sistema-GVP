using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Enums;
using SistemaGVP.Infrastructure.Data.Configurations;

namespace SistemaGVP.Infrastructure.Data;

/// <summary>
/// Contexto principal de Entity Framework Core.
/// Configurado para SQLite con capacidad de migrar a SQL Server.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<InvoiceCounter> InvoiceCounters => Set<InvoiceCounter>();

    /// <summary>
    /// Cuando está en true, se omite la creación de registros de auditoría en SaveChangesAsync.
    /// Útil para operaciones masivas como el seed de datos inicial.
    /// </summary>
    public bool SuppressAudit { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new SaleConfiguration());
        modelBuilder.ApplyConfiguration(new SaleDetailConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryMovementConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceCounterConfiguration());

        // Configuración global: decimal(18,2)
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ConfigureQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&
                       (e.State == EntityState.Modified || e.State == EntityState.Added));

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                ((BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
            }
        }

        // Auditar cambios en entidades BaseEntity (excepto AuditLog mismo)
        if (!SuppressAudit)
        {
            var auditEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity
                           && e.Entity is not AuditLog
                           && (e.State == EntityState.Added
                               || e.State == EntityState.Modified
                               || e.State == EntityState.Deleted))
                .ToList();

            List<AuditLog> auditLogs = new();

            foreach (var entry in auditEntries)
            {
                var entity = (BaseEntity)entry.Entity;
                var entityType = entry.Entity.GetType().Name;
                var entityId = entity.Id;

                string? oldValues = null;
                string? newValues = null;
                AuditAction action;

                switch (entry.State)
                {
                    case EntityState.Added:
                        action = AuditAction.Create;
                        newValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                        break;
                    case EntityState.Modified:
                        action = AuditAction.Update;
                        oldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                        newValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                        break;
                    case EntityState.Deleted:
                        action = AuditAction.Delete;
                        oldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                        break;
                    default:
                        continue;
                }

                // Extraer CompanyId dinámicamente de la entidad auditada si tiene esa propiedad
                var companyId = 0;
                var companyIdProperty = entry.Entity.GetType().GetProperty("CompanyId");
                if (companyIdProperty != null)
                {
                    var value = companyIdProperty.GetValue(entry.Entity);
                    if (value is int intValue)
                        companyId = intValue;
                }

                // Si no se pudo determinar el CompanyId, omitir la auditoría de esta entidad
                // para evitar violaciones de FK (ej: SaleDetail no tiene CompanyId propio,
                // pertenece a una Sale que sí lo tiene, pero no se audita individualmente)
                if (companyId == 0)
                    continue;

                var userId = _currentUser?.IsAuthenticated == true ? _currentUser.UserId : 0;
                var userName = _currentUser?.IsAuthenticated == true ? _currentUser.UserName : "System";

                auditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    Action = action,
                    EntityName = entityType,
                    EntityId = entityId > 0 ? entityId : null,
                    OldValues = oldValues,
                    NewValues = newValues,
                    IpAddress = null,
                    CompanyId = companyId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            foreach (var auditLog in auditLogs)
            {
                AuditLogs.Add(auditLog);
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static void ConfigureQueryFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => e.IsActive);
    }
}
