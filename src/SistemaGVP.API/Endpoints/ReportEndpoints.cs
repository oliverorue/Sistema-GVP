using SistemaGVP.Application.Common;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").RequireAuthorization();

        group.MapGet("/sales", async (DateTime? from, DateTime? to, IReportService service, ICurrentUserService currentUser) =>
        {
            var startDate = (from ?? DateTime.Today).Date;
            var endDate = (to ?? DateTime.Today).Date.AddDays(1).AddSeconds(-1);
            var result = await service.GetSalesByPeriodAsync(currentUser.CompanyId, startDate, endDate);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/low-stock", async (IReportService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetLowStockProductsAsync(currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/profit", async (DateTime? from, DateTime? to, IReportService service, ICurrentUserService currentUser) =>
        {
            var startDate = (from ?? DateTime.Today.AddMonths(-1)).Date;
            var endDate = (to ?? DateTime.Today).Date.AddDays(1).AddSeconds(-1);
            var result = await service.GetProfitMarginAsync(currentUser.CompanyId, startDate, endDate);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/inventory-value", async (IReportService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetInventoryValuationAsync(currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/export", async (
            string type,
            string format,
            DateTime? from,
            DateTime? to,
            IReportService service,
            ICurrentUserService currentUser) =>
        {
            if (format != "excel" && format != "pdf")
                return Results.BadRequest(new { isSuccess = false, message = "Formato debe ser 'excel' o 'pdf'." });

            var startDate = (from ?? DateTime.Today.AddMonths(-1)).Date;
            var endDate = (to ?? DateTime.Today).Date.AddDays(1).AddSeconds(-1);

            ServiceResult<byte[]>? exportResult = null;
            string fileExt;
            string contentType;

            switch (type)
            {
                case "sales":
                    var salesResult = await service.GetSalesByPeriodAsync(currentUser.CompanyId, startDate, endDate);
                    if (!salesResult.IsSuccess)
                        return Results.Ok(new { isSuccess = false, message = salesResult.Message });
                    exportResult = format == "excel"
                        ? await service.ExportReportToExcelAsync(salesResult.Data!, type)
                        : await service.ExportReportToPdfAsync(salesResult.Data!, type);
                    break;

                case "low-stock":
                    var lowStockResult = await service.GetLowStockProductsAsync(currentUser.CompanyId);
                    if (!lowStockResult.IsSuccess)
                        return Results.Ok(new { isSuccess = false, message = lowStockResult.Message });
                    exportResult = format == "excel"
                        ? await service.ExportReportToExcelAsync(lowStockResult.Data!, type)
                        : await service.ExportReportToPdfAsync(lowStockResult.Data!, type);
                    break;

                case "profit":
                    var profitResult = await service.GetProfitMarginAsync(currentUser.CompanyId, startDate, endDate);
                    if (!profitResult.IsSuccess)
                        return Results.Ok(new { isSuccess = false, message = profitResult.Message });
                    exportResult = format == "excel"
                        ? await service.ExportReportToExcelAsync(new List<object> { profitResult.Data! }, type)
                        : await service.ExportReportToPdfAsync(new List<object> { profitResult.Data! }, type);
                    break;

                case "inventory-value":
                    var inventoryResult = await service.GetInventoryValuationAsync(currentUser.CompanyId);
                    if (!inventoryResult.IsSuccess)
                        return Results.Ok(new { isSuccess = false, message = inventoryResult.Message });
                    exportResult = format == "excel"
                        ? await service.ExportReportToExcelAsync(inventoryResult.Data!, type)
                        : await service.ExportReportToPdfAsync(inventoryResult.Data!, type);
                    break;

                default:
                    return Results.BadRequest(new { isSuccess = false, message = "Tipo de reporte no válido." });
            }

            if (exportResult == null || !exportResult.IsSuccess)
                return Results.Ok(new { isSuccess = false, message = "Error al exportar reporte." });

            fileExt = format == "excel" ? "csv" : "html";
            contentType = format == "excel" ? "text/csv; charset=utf-8" : "text/html; charset=utf-8";
            var fileName = $"reporte_{type}_{DateTime.Now:yyyyMMdd}.{fileExt}";

            return Results.File(exportResult.Data!, contentType, fileName);
        });

        return app;
    }
}
