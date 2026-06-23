using System.Reflection;
using System.Text;
using SistemaGVP.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace SistemaGVP.Infrastructure.Services;

public class PdfReportService : IPdfReportService
{
    private readonly ILogger<PdfReportService> _logger;

    public PdfReportService(ILogger<PdfReportService> logger)
    {
        _logger = logger;
    }

    public byte[] ExportToPdf<T>(List<T> data, string reportName)
    {
        try
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var displayName = reportName switch
            {
                "sales" => "Ventas por Período",
                "low-stock" => "Productos con Stock Bajo",
                "profit" => "Margen de Ganancia",
                "inventory-value" => "Valorización de Inventario",
                _ => reportName
            };

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head><meta charset='UTF-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width,initial-scale=1'>");
            html.AppendLine($"<title>{displayName}</title>");
            html.AppendLine("<style>");
            html.AppendLine("@page { margin: 1.5cm; size: A4 landscape; }");
            html.AppendLine("* { box-sizing: border-box; }");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; color: #1e293b; margin: 0; font-size: 10pt; }");
            html.AppendLine(".header { border-bottom: 3px solid #6366f1; padding-bottom: 12px; margin-bottom: 20px; }");
            html.AppendLine(".header h1 { font-size: 18pt; color: #312e81; margin: 0 0 4px 0; }");
            html.AppendLine(".header .meta { font-size: 9pt; color: #64748b; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 10px; font-size: 9pt; }");
            html.AppendLine("thead th { background: linear-gradient(180deg, #6366f1, #4f46e5); color: white; padding: 10px 8px; text-align: left; font-weight: 600; letter-spacing: .5px; }");
            html.AppendLine("thead th.right { text-align: right; }");
            html.AppendLine("tbody td { padding: 8px; border-bottom: 1px solid #e2e8f0; }");
            html.AppendLine("tbody td.right { text-align: right; font-variant-numeric: tabular-nums; }");
            html.AppendLine("tbody tr:nth-child(even) { background-color: #f8fafc; }");
            html.AppendLine("tbody tr:hover { background-color: #eef2ff; }");
            html.AppendLine(".summary { margin-bottom: 20px; }");
            html.AppendLine(".summary .card { display: inline-block; background: linear-gradient(135deg, #eef2ff, #e0e7ff); padding: 12px 20px; margin-right: 10px; margin-bottom: 8px; border-radius: 8px; min-width: 120px; }");
            html.AppendLine(".summary .card .label { font-size: 8pt; color: #6366f1; text-transform: uppercase; letter-spacing: 1px; font-weight: 600; }");
            html.AppendLine(".summary .card .value { font-size: 14pt; font-weight: 700; color: #1e293b; }");
            html.AppendLine(".footer { margin-top: 30px; padding-top: 10px; border-top: 1px solid #e2e8f0; font-size: 8pt; color: #94a3b8; text-align: center; }");
            html.AppendLine("</style></head><body>");

            // Header
            html.AppendLine("<div class='header'>");
            html.AppendLine($"<h1>{displayName}</h1>");
            html.AppendLine($"<div class='meta'>Generado el {DateTime.Now:dd/MM/yyyy HH:mm} · {data.Count} registro(s)</div>");
            html.AppendLine("</div>");

            // Summary card for totals (if numeric properties exist)
            var numericProps = properties.Where(p =>
                p.PropertyType == typeof(decimal) || p.PropertyType == typeof(int) ||
                p.PropertyType == typeof(double) || p.PropertyType == typeof(float));
            if (numericProps.Any() && data.Count > 0)
            {
                html.AppendLine("<div class='summary'>");
                foreach (var prop in numericProps)
                {
                    var sum = data.Sum(item =>
                    {
                        var v = prop.GetValue(item);
                        return v is decimal d ? (double)d :
                               v is int i ? i :
                               v is double db ? db :
                               v is float f ? f : 0;
                    });
                    var label = prop.Name switch
                    {
                        "TotalAmount" => "Total Facturado",
                        "TotalSales" => "Total Ventas",
                        "TotalValue" => "Valor Total",
                        "TotalCost" => "Costo Total",
                        "TotalRevenue" => "Ingreso Total",
                        "Profit" => "Ganancia",
                        "Margin" => "Margen",
                        _ => prop.Name
                    };
                    var formatted = prop.Name == "Margin" ? $"{sum:F1}%" : $"{sum:N0}";
                    html.AppendLine($"<div class='card'><div class='label'>{label}</div><div class='value'>{formatted}</div></div>");
                }
                html.AppendLine("</div>");
            }

            // Table header
            html.AppendLine("<table><thead><tr>");
            foreach (var prop in properties)
            {
                var label = propertyLabel(prop.Name);
                var cls = isRightAligned(prop) ? " class='right'" : "";
                html.AppendLine($"<th{cls}>{label}</th>");
            }
            html.AppendLine("</tr></thead><tbody>");

            // Table data
            foreach (var item in data)
            {
                html.AppendLine("<tr>");
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    var display = formatCell(value, prop);
                    var cls = isRightAligned(prop) ? " class='right'" : "";
                    html.AppendLine($"<td{cls}>{display}</td>");
                }
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table>");

            // Footer
            html.AppendLine($"<div class='footer'>Sistema GVP · Reporte exportado el {DateTime.Now:dd/MM/yyyy HH:mm} · {data.Count} registros</div>");
            html.AppendLine("</body></html>");

            _logger.LogInformation("Reporte HTML generado: {Name}, {Count} registros", displayName, data.Count);
            return Encoding.UTF8.GetBytes(html.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte HTML");
            throw;
        }
    }

    private static string propertyLabel(string name) => name switch
    {
        "Date" => "Día",
        "TotalSales" => "Ventas",
        "TotalAmount" => "Total",
        "ItemCount" => "Items",
        "ProductName" => "Producto",
        "ProductId" => "ID",
        "Barcode" => "Código",
        "CurrentStock" => "Stock",
        "MinStock" => "Mínimo",
        "Difference" => "Faltante",
        "TotalRevenue" => "Ingreso Total",
        "TotalCost" => "Costo Total",
        "Profit" => "Ganancia",
        "Margin" => "Margen (%)",
        "UnitCost" => "Costo Unit.",
        "TotalValue" => "Valor Total",
        "Period" => "Período",
        "Category" => "Categoría",
        _ => name
    };

    private static bool isRightAligned(PropertyInfo prop) =>
        prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(int) ||
        prop.PropertyType == typeof(double) || prop.PropertyType == typeof(float) ||
        prop.PropertyType == typeof(long);

    private static string formatCell(object? value, PropertyInfo prop)
    {
        if (value == null) return "---";
        if (prop.Name == "Margin") return $"{value:F1}%";
        if (prop.Name == "Date" && value is DateTime dt) return dt.ToString("dd/MM/yyyy");
        if (value is decimal dec) return dec.ToString("N0");
        if (value is int i) return i.ToString("N0");
        if (value is double db) return db.ToString("N0");
        if (value is DateTime date) return date.ToString("dd/MM/yyyy");
        return System.Net.WebUtility.HtmlEncode(value.ToString()!);
    }
}
