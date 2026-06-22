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
            ICurrentUserService currentUser) =>
        {
            var companyId = currentUser.CompanyId;

            var dailySalesTask = reportService.GetDailySummaryAsync(companyId, DateTime.Today);
            var lowStockCountTask = inventoryService.GetLowStockCountAsync(companyId);
            var topProductsTask = reportService.GetTopProductsAsync(companyId, 5);
            var recentMovementsTask = inventoryService.GetRecentMovementsAsync(companyId, 10);
            var lowStockProductsTask = reportService.GetLowStockProductsAsync(companyId);
            var heldSalesTask = saleRepository.GetHeldSalesAsync(companyId);
            var allCustomersTask = customerRepository.GetAllNoTrackingAsync();
            var allProductsTask = productRepository.GetAllNoTrackingAsync();

            await Task.WhenAll(
                dailySalesTask, lowStockCountTask, topProductsTask, recentMovementsTask,
                lowStockProductsTask, heldSalesTask, allCustomersTask, allProductsTask);

            var dailySales = dailySalesTask.Result;
            var lowStock = lowStockCountTask.Result;
            var topProducts = topProductsTask.Result;
            var recentMovements = recentMovementsTask.Result;
            var lowStockProducts = lowStockProductsTask.Result;
            var heldSales = heldSalesTask.Result;
            var allCustomers = allCustomersTask.Result;
            var allProducts = allProductsTask.Result;

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
                    customerCount = allCustomers?.Count(c => c.IsActive) ?? 0,
                    productCount = allProducts?.Count(p => p.IsActive) ?? 0
                },
                message = ""
            });
        });

        return app;
    }
}
