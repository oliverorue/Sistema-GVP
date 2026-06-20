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
            var startDate = from ?? DateTime.Today;
            var endDate = to ?? DateTime.Today;
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
            var startDate = from ?? DateTime.Today.AddMonths(-1);
            var endDate = to ?? DateTime.Today;
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

            var startDate = from ?? DateTime.Today.AddMonths(-1);
            var endDate = to ?? DateTime.Today;

            var salesResult = await service.GetSalesByPeriodAsync(currentUser.CompanyId, startDate, endDate);
            if (!salesResult.IsSuccess)
                return Results.Ok(new { isSuccess = false, data = (object?)null, message = salesResult.Message, errors = salesResult.Errors });

            ServiceResult<byte[]>? exportResult = null;
            if (format == "excel")
                exportResult = await service.ExportReportToExcelAsync(salesResult.Data!, type);
            else
                exportResult = await service.ExportReportToPdfAsync(salesResult.Data!, type);

            if (exportResult == null || !exportResult.IsSuccess)
                return Results.Ok(new { isSuccess = false, data = (object?)null, message = "Error al exportar reporte.", errors = Array.Empty<string>() });

            var contentType = format == "excel" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "application/pdf";
            var fileName = $"reporte_{type}_{DateTime.Now:yyyyMMdd}.{(format == "excel" ? "xlsx" : "pdf")}";

            return Results.File(exportResult.Data!, contentType, fileName);
        });

        return app;
    }
}
