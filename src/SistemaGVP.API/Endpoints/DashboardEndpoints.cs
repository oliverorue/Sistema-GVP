using SistemaGVP.Application.Interfaces;

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
            ICurrentUserService currentUser) =>
        {
            var companyId = currentUser.CompanyId;

            var dailySalesTask = reportService.GetDailySummaryAsync(companyId, DateTime.Today);
            var lowStockTask = inventoryService.GetLowStockCountAsync(companyId);
            var topProductsTask = reportService.GetTopProductsAsync(companyId, 5);
            var recentMovementsTask = inventoryService.GetRecentMovementsAsync(companyId, 10);

            await Task.WhenAll(dailySalesTask, lowStockTask, topProductsTask, recentMovementsTask);

            var dailySales = dailySalesTask.Result;
            var lowStock = lowStockTask.Result;
            var topProducts = topProductsTask.Result;
            var recentMovements = recentMovementsTask.Result;

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
                    recentMovements = recentMovements.IsSuccess ? recentMovements.Data : null
                },
                message = ""
            });
        });

        return app;
    }
}
