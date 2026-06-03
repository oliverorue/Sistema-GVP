using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.WPF.Services;
using SistemaGVP.WPF.Views;

namespace SistemaGVP.WPF.ViewModels;

/// <summary>
/// ViewModel for the Dashboard view, shown after login.
/// Displays a welcome message with company info, quick stats, alerts, and dynamic metrics.
/// </summary>
public partial class DashboardViewModel : BaseViewModel
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IProductService _productService;
    private readonly ISaleService _saleService;
    private readonly ICategoryService _categoryService;
    private readonly ICustomerService _customerService;
    private readonly IReportService _reportService;
    private readonly IInventoryService _inventoryService;
    private readonly INavigationService _navigationService;

    // ─── Loading State ──────────────────────────────────────────
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasErrors;

    [ObservableProperty]
    private ObservableCollection<string> _loadErrors = new();

    // ─── Welcome ────────────────────────────────────────────────
    [ObservableProperty]
    private string _welcomeMessage = "Bienvenido";

    [ObservableProperty]
    private string _companyName = "Sistema GVP";

    // ─── Stat Cards ─────────────────────────────────────────────
    [ObservableProperty]
    private int _totalProducts;

    [ObservableProperty]
    private int _totalCategories;

    [ObservableProperty]
    private int _totalCustomers;

    [ObservableProperty]
    private int _todaySalesCount;

    [ObservableProperty]
    private string _todaySalesAmount = "Gs. 0";

    // ─── Daily Summary (from IReportService) ────────────────────
    [ObservableProperty]
    private string _dailyRevenue = "Gs. 0";

    [ObservableProperty]
    private string _dailyAverageTicket = "Gs. 0";

    [ObservableProperty]
    private int _dailyItemsSold;

    [ObservableProperty]
    private string _dailyTax = "Gs. 0";

    [ObservableProperty]
    private string _profitMargin = "0%";

    [ObservableProperty]
    private string _profitAmount = "Gs. 0";

    // ─── Top Products ───────────────────────────────────────────
    [ObservableProperty]
    private List<TopProductDto> _topProducts = new();

    [ObservableProperty]
    private bool _hasTopProducts;

    // ─── Low Stock Alerts ───────────────────────────────────────
    [ObservableProperty]
    private List<LowStockProductDto> _lowStockProducts = new();

    [ObservableProperty]
    private int _lowStockCount;

    [ObservableProperty]
    private bool _hasLowStock;

    // ─── Weekly Sales Trend ─────────────────────────────────────
    [ObservableProperty]
    private List<SalesByPeriodDto> _weeklySales = new();

    [ObservableProperty]
    private bool _hasWeeklySales;

    // LiveCharts2 chart series for weekly sales (line chart)
    [ObservableProperty]
    private ISeries[] _weeklySalesSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _weeklySalesXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _weeklySalesYAxes = Array.Empty<Axis>();

    // LiveCharts2 chart series for top products (column chart)
    [ObservableProperty]
    private ISeries[] _topProductsSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _topProductsXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _topProductsYAxes = Array.Empty<Axis>();

    // ─── Yesterday Comparison ───────────────────────────────────
    [ObservableProperty]
    private string _yesterdaySalesAmount = "Gs. 0";

    [ObservableProperty]
    private string _salesComparison = "0%";

    [ObservableProperty]
    private bool _salesUp;

    public DashboardViewModel(
        ICurrentUserService currentUserService,
        IProductService productService,
        ISaleService saleService,
        ICategoryService categoryService,
        ICustomerService customerService,
        IReportService reportService,
        IInventoryService inventoryService,
        INavigationService navigationService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _currentUserService = currentUserService;
        _productService = productService;
        _saleService = saleService;
        _categoryService = categoryService;
        _customerService = customerService;
        _reportService = reportService;
        _inventoryService = inventoryService;
        _navigationService = navigationService;
        ViewTitle = "Dashboard";
    }

    public override async Task LoadAsync()
    {
        IsLoading = true;
        HasErrors = false;
        LoadErrors.Clear();

        try
        {
            var userName = _currentUserService.UserName;
            WelcomeMessage = $"Bienvenido, {userName}";

            var companyId = _currentUserService.CompanyId;

            // Load everything in parallel
            await Task.WhenAll(
                LoadProductCountAsync(companyId),
                LoadCategoryCountAsync(companyId),
                LoadCustomerCountAsync(companyId),
                LoadTodaySalesAsync(companyId),
                LoadDailySummaryAsync(companyId),
                LoadTopProductsAsync(companyId),
                LoadLowStockAsync(companyId),
                LoadWeeklySalesAsync(companyId),
                LoadYesterdayComparisonAsync(companyId)
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error crítico al cargar dashboard");
            HasErrors = true;
            LoadErrors.Add($"Error crítico: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ─── Navigation Commands ────────────────────────────────────
    [RelayCommand]
    private async Task NavigateToInventory()
    {
        await _navigationService.NavigateTo<InventoryView>();
    }

    [RelayCommand]
    private async Task NavigateToProducts()
    {
        await _navigationService.NavigateTo<ProductsView>();
    }

    // ─── Private Loaders ────────────────────────────────────────

    private async Task LoadProductCountAsync(int companyId)
    {
        try
        {
            var result = await _productService.GetAllAsync(new Application.Common.PaginationFilter(1, 1), companyId);
            if (result.IsSuccess && result.Data != null)
                TotalProducts = result.Data.TotalCount;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al cargar conteo de productos");
            LoadErrors.Add($"Productos: {ex.Message}");
            HasErrors = true;
        }
    }

    private async Task LoadCategoryCountAsync(int companyId)
    {
        try
        {
            var result = await _categoryService.GetAllAsync(companyId);
            if (result.IsSuccess && result.Data != null)
                TotalCategories = result.Data.Count;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al cargar conteo de categorías");
            LoadErrors.Add($"Categorías: {ex.Message}");
            HasErrors = true;
        }
    }

    private async Task LoadCustomerCountAsync(int companyId)
    {
        try
        {
            var result = await _customerService.SearchAsync("", companyId);
            if (result.IsSuccess && result.Data != null)
                TotalCustomers = result.Data.Count;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al cargar conteo de clientes");
            LoadErrors.Add($"Clientes: {ex.Message}");
            HasErrors = true;
        }
    }

    private async Task LoadTodaySalesAsync(int companyId)
    {
        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var result = await _saleService.GetSalesByDateAsync(companyId, today, tomorrow);
            if (result.IsSuccess && result.Data != null)
            {
                TodaySalesCount = result.Data.Count;
                var total = result.Data.Sum(s => s.Total);
                TodaySalesAmount = $"Gs. {total:N0}";
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al cargar ventas del día");
            LoadErrors.Add($"Ventas del día: {ex.Message}");
            HasErrors = true;
        }
    }

    private async Task LoadDailySummaryAsync(int companyId)
    {
        try
        {
            var result = await _reportService.GetDailySummaryAsync(companyId, DateTime.Today);
            if (result.IsSuccess && result.Data != null)
            {
                DailyRevenue = $"Gs. {result.Data.TotalRevenue:N0}";
                DailyAverageTicket = $"Gs. {result.Data.AverageTicket:N0}";
                DailyItemsSold = result.Data.TotalItems;
                DailyTax = $"Gs. {result.Data.TotalTax:N0}";
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al cargar resumen diario");
            LoadErrors.Add($"Resumen diario: {ex.Message}");
            HasErrors = true;
        }
    }

    private async Task LoadTopProductsAsync(int companyId)
    {
        try
        {
            var result = await _reportService.GetTopProductsAsync(companyId, 5);
            if (result.IsSuccess && result.Data != null)
            {
                TopProducts = result.Data;
                HasTopProducts = result.Data.Count > 0;

                // Populate LiveCharts2 column series for top products
                var values = result.Data.Select(p => (double)p.TotalQuantity).ToArray();
                var labels = result.Data.Select(p => p.ProductName.Length > 12
                    ? p.ProductName[..12] + "…" : p.ProductName).ToArray();

                TopProductsSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = values,
                        Fill = new SolidColorPaint(SKColor.Parse("#1976D2")),
                        Stroke = null
                    }
                };

                TopProductsXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsRotation = 0,
                        TextSize = 11,
                        LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B"))
                    }
                };

                TopProductsYAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = "Unidades",
                        NameTextSize = 11,
                        NamePaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                        TextSize = 11,
                        LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B")),
                        MinLimit = 0
                    }
                };
            }
            else
            {
                TopProductsSeries = Array.Empty<ISeries>();
                TopProductsXAxes = Array.Empty<Axis>();
                TopProductsYAxes = Array.Empty<Axis>();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al cargar top productos");
            LoadErrors.Add($"Top productos: {ex.Message}");
            HasErrors = true;
        }
    }

    private async Task LoadLowStockAsync(int companyId)
    {
        try
        {
            var result = await _reportService.GetLowStockProductsAsync(companyId);
            if (result.IsSuccess && result.Data != null)
            {
                LowStockProducts = result.Data;
                LowStockCount = result.Data.Count;
                HasLowStock = result.Data.Count > 0;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al cargar stock bajo");
            LoadErrors.Add($"Stock bajo: {ex.Message}");
            HasErrors = true;
        }
    }

    private async Task LoadWeeklySalesAsync(int companyId)
    {
        try
        {
            var weekAgo = DateTime.Today.AddDays(-6);
            var today = DateTime.Today.AddDays(1);
            var result = await _reportService.GetSalesByPeriodAsync(companyId, weekAgo, today);
            if (result.IsSuccess && result.Data != null)
            {
                WeeklySales = result.Data;
                HasWeeklySales = result.Data.Count > 0;

                // Populate LiveCharts2 line series for weekly sales
                var values = result.Data.Select(s => (double)s.TotalAmount).ToArray();
                var labels = result.Data.Select(s => s.Date.ToString("ddd dd/MM")).ToArray();

                WeeklySalesSeries = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = values,
                        Fill = new SolidColorPaint(SKColor.Parse("#0D47A1").WithAlpha(50)),
                        Stroke = new SolidColorPaint(SKColor.Parse("#0D47A1"), 3),
                        GeometryFill = new SolidColorPaint(SKColor.Parse("#FFFFFF")),
                        GeometryStroke = new SolidColorPaint(SKColor.Parse("#0D47A1"), 2),
                        GeometrySize = 10,
                        LineSmoothness = 0.4
                    }
                };

                WeeklySalesXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsRotation = -30,
                        TextSize = 11,
                        LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B"))
                    }
                };

                WeeklySalesYAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = "Gs.",
                        NameTextSize = 11,
                        NamePaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                        TextSize = 11,
                        LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B")),
                        Labeler = value => value >= 1_000_000
                            ? $"{(value / 1_000_000):N1}M"
                            : $"{(value / 1_000):N0}K",
                        MinLimit = 0
                    }
                };
            }
            else
            {
                WeeklySalesSeries = Array.Empty<ISeries>();
                WeeklySalesXAxes = Array.Empty<Axis>();
                WeeklySalesYAxes = Array.Empty<Axis>();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al cargar ventas semanales");
            LoadErrors.Add($"Ventas semanales: {ex.Message}");
            HasErrors = true;
        }
    }

    private async Task LoadYesterdayComparisonAsync(int companyId)
    {
        try
        {
            var yesterdayStart = DateTime.Today.AddDays(-1);
            var yesterdayEnd = DateTime.Today;

            var yesterdayResult = await _reportService.GetDailySummaryAsync(companyId, yesterdayStart);
            var todayResult = await _reportService.GetDailySummaryAsync(companyId, DateTime.Today);

            decimal yesterdayRevenue = yesterdayResult.IsSuccess ? yesterdayResult.Data?.TotalRevenue ?? 0 : 0;
            decimal todayRevenue = todayResult.IsSuccess ? todayResult.Data?.TotalRevenue ?? 0 : 0;

            YesterdaySalesAmount = $"Gs. {yesterdayRevenue:N0}";

            if (yesterdayRevenue > 0)
            {
                var diff = ((todayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100;
                SalesComparison = $"{(diff >= 0 ? "+" : "")}{diff:N1}%";
                SalesUp = diff >= 0;
            }
            else if (todayRevenue > 0)
            {
                SalesComparison = "+100%";
                SalesUp = true;
            }
            else
            {
                SalesComparison = "0%";
                SalesUp = true;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al cargar comparativa con ayer");
            LoadErrors.Add($"Comparativa ayer: {ex.Message}");
            HasErrors = true;
        }
    }
}
