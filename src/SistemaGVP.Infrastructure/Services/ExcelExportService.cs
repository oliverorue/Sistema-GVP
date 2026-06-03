using System.Reflection;
using System.Text;
using SistemaGVP.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace SistemaGVP.Infrastructure.Services;

/// <summary>
/// Servicio de exportación a CSV (compatible con Excel).
/// Utiliza System.IO para generar archivos CSV sin dependencias externas.
/// </summary>
public class ExcelExportService : IExcelExportService
{
    private readonly ILogger<ExcelExportService> _logger;

    public ExcelExportService(ILogger<ExcelExportService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Exporta una lista de datos a CSV y retorna los bytes.
    /// </summary>
    public byte[] ExportToBytes<T>(List<T> data)
    {
        try
        {
            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Header
            var header = string.Join(",", properties.Select(p => EscapeCsvField(p.Name)));
            sb.AppendLine(header);

            // Data rows
            foreach (var item in data)
            {
                var row = string.Join(",", properties.Select(p =>
                {
                    var value = p.GetValue(item);
                    return EscapeCsvField(value?.ToString() ?? string.Empty);
                }));
                sb.AppendLine(row);
            }

            _logger.LogInformation("Exportación CSV generada: {Count} registros, {Props} columnas",
                data.Count, properties.Length);

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar datos a CSV");
            throw;
        }
    }

    /// <summary>
    /// Exporta una lista de datos a un archivo CSV en disco.
    /// </summary>
    public async Task ExportReportAsync<T>(List<T> data, string filePath)
    {
        var bytes = ExportToBytes(data);
        await File.WriteAllBytesAsync(filePath, bytes);
        _logger.LogInformation("Archivo CSV exportado a: {Path}", filePath);
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
