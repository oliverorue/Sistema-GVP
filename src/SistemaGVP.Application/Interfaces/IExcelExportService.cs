namespace SistemaGVP.Application.Interfaces;

/// <summary>
/// Servicio de exportación a Excel/CSV.
/// </summary>
public interface IExcelExportService
{
    byte[] ExportToBytes<T>(List<T> data);
    Task ExportReportAsync<T>(List<T> data, string filePath);
}
