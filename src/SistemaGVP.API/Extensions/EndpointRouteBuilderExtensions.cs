using SistemaGVP.API.Endpoints;
using SistemaGVP.API.Middleware;

namespace SistemaGVP.API.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapProductEndpoints();
        app.MapCategoryEndpoints();
        app.MapCustomerEndpoints();
        app.MapSupplierEndpoints();
        app.MapSaleEndpoints();
        app.MapInventoryEndpoints();
        app.MapReportEndpoints();
        app.MapUserEndpoints();
        app.MapSettingsEndpoints();
        app.MapAuditEndpoints();
        app.MapBackupEndpoints();
        app.MapDashboardEndpoints();
        return app;
    }

    public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder app)
    {
        app.UseExceptionHandling();
        app.UseSecurityHeaders();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
