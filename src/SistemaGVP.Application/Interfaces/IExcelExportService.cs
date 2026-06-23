namespace SistemaGVP.Application.Interfaces;

public interface IExcelExportService
{
    byte[] ExportToBytes<T>(List<T> data, Dictionary<string, string>? headers = null);
    Task ExportReportAsync<T>(List<T> data, string filePath);
}
