using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").RequireAuthorization();

        group.MapGet("/summary", async (
            IReportService reportService,
            IInventoryService inventoryService,
            IProductService productService,
            ISaleRepository saleRepository,
            IRepository<Domain.Entities.Customer> customerRepository,
            IRepository<Domain.Entities.Product> productRepository,
            ICurrentUserService currentUser,
            ILogger<Program> logger) =>
        {
            try
            {
                var companyId = currentUser.CompanyId;

                var dailySalesTask = reportService.GetDailySummaryAsync(companyId, DateTime.Today);
                var lowStockCountTask = inventoryService.GetLowStockCountAsync(companyId);
                var topProductsTask = reportService.GetTopProductsAsync(companyId, 5);
                var recentMovementsTask = inventoryService.GetRecentMovementsAsync(companyId, 10);
                var lowStockProductsTask = reportService.GetLowStockProductsAsync(companyId);
                var heldSalesTask = saleRepository.GetHeldSalesAsync(companyId);

                // Use company-filtered queries instead of GetAllNoTrackingAsync
                var productCountTask = productService.GetAllAsync(new PaginationFilter(1, 1), companyId);
                var customersTask = customerRepository.GetAllNoTrackingAsync();

                await Task.WhenAll(
                    dailySalesTask, lowStockCountTask, topProductsTask, recentMovementsTask,
                    lowStockProductsTask, heldSalesTask, productCountTask, customersTask);

                var dailySales = dailySalesTask.Result;
                var lowStock = lowStockCountTask.Result;
                var topProducts = topProductsTask.Result;
                var recentMovements = recentMovementsTask.Result;
                var lowStockProducts = lowStockProductsTask.Result;
                var heldSales = heldSalesTask.Result;

                // Get filtered counts using the service/repository properly
                var allCustomers = customersTask.Result;
                var productResult = productCountTask.Result;

                int productCount = 0;
                if (productResult.IsSuccess && productResult.Data != null)
                {
                    productCount = productResult.Data.TotalCount;
                }

                int customerCount = allCustomers?.Count(c => c.IsActive && c.CompanyId == companyId) ?? 0;

                logger.LogInformation(
                    "Dashboard summary for company {CompanyId}: products={ProductCount}, customers={CustomerCount}",
                    companyId, productCount, customerCount);

                return Results.Ok(new
                {
                    isSuccess = true,
                    data = new
                    {
                        todaySales = dailySales.IsSuccess ? new
                        {
                            dailySales.Data!.TotalSales,
                            dailySales.Data.TotalRevenue,
                            dailySales.Data.TotalTax,
                            dailySales.Data.AverageTicket
                        } : null,
                        lowStockCount = lowStock.IsSuccess ? lowStock.Data : 0,
                        topProducts = topProducts.IsSuccess ? topProducts.Data : null,
                        recentMovements = recentMovements.IsSuccess ? recentMovements.Data : null,
                        lowStockProducts = lowStockProducts.IsSuccess ? lowStockProducts.Data : null,
                        heldSalesCount = heldSales?.Count ?? 0,
                        customerCount,
                        productCount
                    },
                    message = ""
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al generar resumen del dashboard para empresa {CompanyId}", currentUser.CompanyId);
                return Results.Ok(new
                {
                    isSuccess = false,
                    data = (object?)null,
                    message = "Error al cargar el resumen del dashboard.",
                    errors = new[] { ex.Message }
                });
            }
        });

        return app;
    }
}
