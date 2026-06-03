namespace SistemaGVP.Application.Interfaces;

/// <summary>
/// Servicio de exportación a PDF.
/// </summary>
public interface IPdfReportService
{
    byte[] ExportToPdf<T>(List<T> data, string reportName);
}
