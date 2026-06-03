using System.Reflection;
using System.Text;
using SistemaGVP.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace SistemaGVP.Infrastructure.Services;

/// <summary>
/// Servicio de exportación a PDF utilizando HTML formateado.
/// Genera un documento HTML que puede visualizarse como PDF o imprimirse.
/// </summary>
public class PdfReportService : IPdfReportService
{
    private readonly ILogger<PdfReportService> _logger;

    public PdfReportService(ILogger<PdfReportService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Genera un reporte HTML formateado como representación de PDF.
    /// Retorna los bytes del HTML (puede convertirse a PDF con herramientas externas).
    /// </summary>
    public byte[] ExportToPdf<T>(List<T> data, string reportName)
    {
        try
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head><meta charset='UTF-8'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #1a237e; font-size: 18px; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 10px; }");
            html.AppendLine("th { background-color: #1a237e; color: white; padding: 8px; text-align: left; font-size: 12px; }");
            html.AppendLine("td { border: 1px solid #ddd; padding: 6px; font-size: 11px; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f5f5f5; }");
            html.AppendLine(".footer { margin-top: 20px; font-size: 10px; color: #666; text-align: center; }");
            html.AppendLine("</style></head><body>");

            html.AppendLine($"<h1>{reportName}</h1>");
            html.AppendLine($"<p>Generado: {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            html.AppendLine("<table>");

            // Header
            html.AppendLine("<tr>");
            foreach (var prop in properties)
            {
                html.AppendLine($"<th>{prop.Name}</th>");
            }
            html.AppendLine("</tr>");

            // Data
            foreach (var item in data)
            {
                html.AppendLine("<tr>");
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item)?.ToString() ?? string.Empty;
                    html.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(value)}</td>");
                }
                html.AppendLine("</tr>");
            }

            html.AppendLine("</table>");
            html.AppendLine($"<div class='footer'>Sistema GVP - {data.Count} registro(s)</div>");
            html.AppendLine("</body></html>");

            _logger.LogInformation("Reporte PDF generado: {Name}, {Count} registros", reportName, data.Count);

            return Encoding.UTF8.GetBytes(html.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte PDF");
            throw;
        }
    }
}
