using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.WPF.Services;

namespace SistemaGVP.WPF.ViewModels;

/// <summary>
/// ViewModel para la generación y exportación de reportes.
/// Soport 6 tipos de reportes con filtros de fecha y exportación Excel/PDF.
/// </summary>
public partial class ReportsViewModel : BaseViewModel
{
    private readonly IReportService _reportService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IExcelExportService _excelExportService;
    private readonly IPdfReportService _pdfReportService;
    private readonly IDialogService _dialogService;

    // ──────────────────────────────────────────────
    //  Propiedades de selección y filtros
    // ──────────────────────────────────────────────

    [ObservableProperty]
    private string _selectedReportType = "Ventas Diarias";

    [ObservableProperty]
    private List<string> _reportTypes =
    [
        "Ventas Diarias",
        "Productos Más Vendidos",
        "Stock Bajo",
        "Ventas por Período",
        "Márgenes de Ganancia",
        "Valoración de Inventario"
    ];

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    // ──────────────────────────────────────────────
    //  Colecciones tipadas para cada reporte
    // ──────────────────────────────────────────────

    [ObservableProperty]
    private ObservableCollection<DailySalesSummaryDto> _dailySales = [];

    [ObservableProperty]
    private ObservableCollection<TopProductDto> _topProducts = [];

    [ObservableProperty]
    private ObservableCollection<LowStockProductDto> _lowStockItems = [];

    [ObservableProperty]
    private ObservableCollection<SalesByPeriodDto> _salesByPeriod = [];

    [ObservableProperty]
    private ObservableCollection<ProfitMarginDto> _profitMargins = [];

    [ObservableProperty]
    private ObservableCollection<InventoryValuationDto> _inventoryValuation = [];

    // ──────────────────────────────────────────────
    //  Visibilidad selectiva (bind para DataGrids)
    // ──────────────────────────────────────────────

    [ObservableProperty]
    private bool _isDailySales;

    [ObservableProperty]
    private bool _isTopProducts;

    [ObservableProperty]
    private bool _isLowStock;

    [ObservableProperty]
    private bool _isSalesByPeriod;

    [ObservableProperty]
    private bool _isProfitMargins;

    [ObservableProperty]
    private bool _isInventoryValuation;

    // ──────────────────────────────────────────────
    //  Constructor
    // ──────────────────────────────────────────────

    public ReportsViewModel(
        IReportService reportService,
        ICurrentUserService currentUserService,
        IExcelExportService excelExportService,
        IPdfReportService pdfReportService,
        IDialogService dialogService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _reportService = reportService;
        _currentUserService = currentUserService;
        _excelExportService = excelExportService;
        _pdfReportService = pdfReportService;
        _dialogService = dialogService;
        ViewTitle = "Reportes";
    }

    // ──────────────────────────────────────────────
    //  Inicialización
    // ──────────────────────────────────────────────

    public override async Task LoadAsync()
    {
        // Rango default: último mes
        StartDate = DateTime.Today.AddMonths(-1);
        EndDate = DateTime.Today;
        await SetTemporaryStatusAsync("Listo. Seleccione un reporte y presione Generar.", 2000);
    }

    // ──────────────────────────────────────────────
    //  Cambio de tipo de reporte → limpia colección
    // ──────────────────────────────────────────────

    partial void OnSelectedReportTypeChanged(string value)
    {
        ClearAllCollections();
        ClearAllVisibility();
    }

    // ──────────────────────────────────────────────
    //  GenerarReporteCommand
    // ──────────────────────────────────────────────

    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var companyId = _currentUserService.CompanyId;

            _logger?.LogInformation("GenerateReportAsync: Iniciando generación de reporte '{ReportType}' para CompanyId={CompanyId}",
                SelectedReportType, companyId);

            ClearAllCollections();
            ClearAllVisibility();

            switch (SelectedReportType)
            {
                case "Ventas Diarias":
                    await GenerateDailySalesAsync(companyId);
                    break;
                case "Productos Más Vendidos":
                    await GenerateTopProductsAsync(companyId);
                    break;
                case "Stock Bajo":
                    await GenerateLowStockAsync(companyId);
                    break;
                case "Ventas por Período":
                    await GenerateSalesByPeriodAsync(companyId);
                    break;
                case "Márgenes de Ganancia":
                    await GenerateProfitMarginAsync(companyId);
                    break;
                case "Valoración de Inventario":
                    await GenerateInventoryValuationAsync(companyId);
                    break;
            }

            await SetTemporaryStatusAsync($"✅ Reporte '{SelectedReportType}' generado correctamente", 3000);
        }, "Generar reporte");
    }

    private async Task GenerateDailySalesAsync(int companyId)
    {
        var date = StartDate ?? DateTime.Today;
        var result = await _reportService.GetDailySummaryAsync(companyId, date);

        if (!result.IsSuccess || result.Data is null)
        {
            HasError = true;
            ErrorMessage = result.Message;
            return;
        }

        DailySales = new ObservableCollection<DailySalesSummaryDto> { result.Data };
        IsDailySales = true;
    }

    private async Task GenerateTopProductsAsync(int companyId)
    {
        var result = await _reportService.GetTopProductsAsync(companyId, 10);

        if (!result.IsSuccess)
        {
            HasError = true;
            ErrorMessage = result.Message;
            return;
        }

        TopProducts = new ObservableCollection<TopProductDto>(result.Data ?? []);
        IsTopProducts = TopProducts.Count > 0;
    }

    private async Task GenerateLowStockAsync(int companyId)
    {
        var result = await _reportService.GetLowStockProductsAsync(companyId);

        if (!result.IsSuccess)
        {
            HasError = true;
            ErrorMessage = result.Message;
            return;
        }

        LowStockItems = new ObservableCollection<LowStockProductDto>(result.Data ?? []);
        IsLowStock = LowStockItems.Count > 0;
    }

    private async Task GenerateSalesByPeriodAsync(int companyId)
    {
        var start = StartDate ?? DateTime.Today.AddMonths(-1);
        var end = EndDate ?? DateTime.Today;

        var result = await _reportService.GetSalesByPeriodAsync(companyId, start, end);

        if (!result.IsSuccess)
        {
            HasError = true;
            ErrorMessage = result.Message;
            return;
        }

        SalesByPeriod = new ObservableCollection<SalesByPeriodDto>(result.Data ?? []);
        IsSalesByPeriod = SalesByPeriod.Count > 0;
    }

    private async Task GenerateProfitMarginAsync(int companyId)
    {
        var start = StartDate ?? DateTime.Today.AddMonths(-1);
        var end = EndDate ?? DateTime.Today;

        var result = await _reportService.GetProfitMarginAsync(companyId, start, end);

        if (!result.IsSuccess || result.Data is null)
        {
            HasError = true;
            ErrorMessage = result.Message;
            return;
        }

        ProfitMargins = new ObservableCollection<ProfitMarginDto> { result.Data };
        IsProfitMargins = true;
    }

    private async Task GenerateInventoryValuationAsync(int companyId)
    {
        var result = await _reportService.GetInventoryValuationAsync(companyId);

        if (!result.IsSuccess)
        {
            HasError = true;
            ErrorMessage = result.Message;
            return;
        }

        InventoryValuation = new ObservableCollection<InventoryValuationDto>(result.Data ?? []);
        IsInventoryValuation = InventoryValuation.Count > 0;
    }

    // ──────────────────────────────────────────────
    //  Exportar a Excel
    // ──────────────────────────────────────────────

    [RelayCommand]
    private async Task ExportToExcelAsync()
    {
        await ExportToFileAsync(isExcel: true);
    }

    // ──────────────────────────────────────────────
    //  Exportar a PDF
    // ──────────────────────────────────────────────

    [RelayCommand]
    private async Task ExportToPdfAsync()
    {
        await ExportToFileAsync(isExcel: false);
    }

    // ──────────────────────────────────────────────
    //  Helpers de exportación
    // ──────────────────────────────────────────────

    /// <summary>
    /// Obtiene los datos visibles actuales y dispara la exportación tipada.
    /// </summary>
    private async Task ExportToFileAsync(bool isExcel)
    {
        await ExecuteSafeAsync(async () =>
        {
            var data = GetCurrentExportDataAsObject();

            if (data is null)
            {
                await _dialogService.ShowWarningAsync(
                    "No hay datos para exportar. Genere un reporte primero.",
                    "Sin datos");
                return;
            }

            var format = isExcel ? "Excel" : "PDF";
            var extension = isExcel ? ".xlsx" : ".pdf";
            var filter = isExcel
                ? "Archivos Excel (*.xlsx)|*.xlsx"
                : "Archivos PDF (*.pdf)|*.pdf";

            var defaultName = $"Reporte_{SanitizeFileName(SelectedReportType)}_{DateTime.Today:yyyyMMdd}";
            string? filePath = null;

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new SaveFileDialog
                {
                    FileName = defaultName,
                    DefaultExt = extension,
                    Filter = filter,
                    Title = $"Exportar a {format}"
                };

                if (dialog.ShowDialog() == true)
                {
                    filePath = dialog.FileName;
                }
            });

            if (string.IsNullOrEmpty(filePath)) return;

            // Resolver el tipo concreto en runtime para llamar al servicio genérico
            switch (data)
            {
                case List<DailySalesSummaryDto> d:
                    await ExportTypedAsync(d, filePath, isExcel);
                    break;
                case List<TopProductDto> d:
                    await ExportTypedAsync(d, filePath, isExcel);
                    break;
                case List<LowStockProductDto> d:
                    await ExportTypedAsync(d, filePath, isExcel);
                    break;
                case List<SalesByPeriodDto> d:
                    await ExportTypedAsync(d, filePath, isExcel);
                    break;
                case List<ProfitMarginDto> d:
                    await ExportTypedAsync(d, filePath, isExcel);
                    break;
                case List<InventoryValuationDto> d:
                    await ExportTypedAsync(d, filePath, isExcel);
                    break;
            }

            await SetTemporaryStatusAsync($"✅ Reporte exportado a {format}: {Path.GetFileName(filePath)}", 4000);
        }, isExcel ? "Exportar a Excel" : "Exportar a PDF");
    }

    /// <summary>
    /// Exporta una lista tipada usando el servicio correspondiente.
    /// </summary>
    private async Task ExportTypedAsync<T>(List<T> typedData, string filePath, bool isExcel)
    {
        if (isExcel)
        {
            var bytes = _excelExportService.ExportToBytes(typedData);
            await File.WriteAllBytesAsync(filePath, bytes);
        }
        else
        {
            var reportName = Path.GetFileNameWithoutExtension(filePath);
            var bytes = _pdfReportService.ExportToPdf(typedData, reportName);
            await File.WriteAllBytesAsync(filePath, bytes);
        }
    }

    /// <summary>
    /// Obtiene los datos actualmente visibles como objeto List<T> tipado o null.
    /// </summary>
    private object? GetCurrentExportDataAsObject()
    {
        if (IsDailySales && DailySales.Count > 0)
            return DailySales.ToList();
        if (IsTopProducts && TopProducts.Count > 0)
            return TopProducts.ToList();
        if (IsLowStock && LowStockItems.Count > 0)
            return LowStockItems.ToList();
        if (IsSalesByPeriod && SalesByPeriod.Count > 0)
            return SalesByPeriod.ToList();
        if (IsProfitMargins && ProfitMargins.Count > 0)
            return ProfitMargins.ToList();
        if (IsInventoryValuation && InventoryValuation.Count > 0)
            return InventoryValuation.ToList();

        return null;
    }

    // ──────────────────────────────────────────────
    //  Limpieza
    // ──────────────────────────────────────────────

    private void ClearAllCollections()
    {
        DailySales = [];
        TopProducts = [];
        LowStockItems = [];
        SalesByPeriod = [];
        ProfitMargins = [];
        InventoryValuation = [];
    }

    private void ClearAllVisibility()
    {
        IsDailySales = false;
        IsTopProducts = false;
        IsLowStock = false;
        IsSalesByPeriod = false;
        IsProfitMargins = false;
        IsInventoryValuation = false;
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }
}
