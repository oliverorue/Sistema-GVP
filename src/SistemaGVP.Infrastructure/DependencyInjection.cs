using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Application.Services;
using SistemaGVP.Application.Validators;
using SistemaGVP.Domain.Interfaces;
using SistemaGVP.Infrastructure.Caching;
using SistemaGVP.Infrastructure.Data;
using SistemaGVP.Infrastructure.Data.Seed;
using SistemaGVP.Infrastructure.Logging;
using SistemaGVP.Infrastructure.Repositories;
using SistemaGVP.Infrastructure.Services;

namespace SistemaGVP.Infrastructure;

/// <summary>
/// Registro de dependencias para la capa de Infraestructura.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ==========================================
        // Logger (Serilog) - registra tanto Serilog.ILogger como Microsoft.Extensions.Logging.ILogger<T>
        // ==========================================
        Log.Logger = LoggerSetup.CreateLogger();
        services.AddSingleton(Log.Logger);
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger, dispose: true);
        });

        // ==========================================
        // Base de datos - EF Core con SQLite
        // ==========================================
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? $"Data Source={GetDefaultDatabasePath()}";

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });
        });

        // ==========================================
        // Repositories
        // ==========================================
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInvoiceCounterRepository, InvoiceCounterRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ==========================================
        // Infrastructure Services
        // ==========================================
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddSingleton<CurrentUserService>();
        services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<CurrentUserService>());
        services.AddTransient<IExcelExportService, ExcelExportService>();
        services.AddTransient<IPdfReportService, PdfReportService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IAuditService, AuditService>();

        // ==========================================
        // Application Services
        // ==========================================
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ISettingsService, SettingsService>();

        // ==========================================
        // Caching (In-Memory)
        // ==========================================
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // ==========================================
        // FluentValidation
        // ==========================================
        services.AddScoped<IValidator<ProductDto>, ProductValidator>();
        services.AddScoped<IValidator<UserDto>, UserValidator>();
        services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
        services.AddScoped<IValidator<CustomerDto>, CustomerValidator>();
        services.AddScoped<IValidator<SupplierDto>, SupplierValidator>();
        services.AddScoped<IValidator<CategoryDto>, CategoryValidator>();
        services.AddScoped<IValidator<CreateSaleDto>, CreateSaleValidator>();
        services.AddScoped<IValidator<CreateInventoryMovementDto>, CreateInventoryMovementValidator>();
        services.AddScoped<IValidator<CompanyDto>, CompanyValidator>();

        // ==========================================
        // AutoMapper
        // ==========================================
        services.AddAutoMapper(typeof(Application.Mappers.MappingProfile));

        return services;
    }

    /// <summary>
    /// Inicializa la base de datos con seed data.
    /// </summary>
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<AppDbContext>>();
        await DatabaseSeeder.SeedAsync(context, logger);
    }

    private static string GetDefaultDatabasePath()
    {
        var dbDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SistemaGVP");

        Directory.CreateDirectory(dbDirectory);
        return Path.Combine(dbDirectory, "sistemagvp.db");
    }

}
