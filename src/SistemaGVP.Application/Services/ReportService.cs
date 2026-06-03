using AutoMapper;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Enums;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Services;

public class ReportService : IReportService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IExcelExportService _excelExportService;
    private readonly IPdfReportService _pdfReportService;
    private readonly IMapper _mapper;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IExcelExportService excelExportService,
        IPdfReportService pdfReportService,
        IMapper mapper,
        ILogger<ReportService> logger)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _excelExportService = excelExportService;
        _pdfReportService = pdfReportService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<DailySalesSummaryDto>> GetDailySummaryAsync(int companyId, DateTime date)
    {
        try
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1).AddSeconds(-1);

            var sales = await _saleRepository.GetByDateRangeAsync(companyId, dayStart, dayEnd);
            var completedSales = sales.Where(s => s.Status == SaleStatus.Completed).ToList();

            var summary = new DailySalesSummaryDto
            {
                TotalSales = completedSales.Count,
                TotalRevenue = completedSales.Sum(s => s.Total),
                TotalTax = completedSales.Sum(s => s.Tax),
                TotalItems = completedSales.Sum(s => s.SaleDetails.Sum(d => (int)d.Quantity)),
                AverageTicket = completedSales.Count > 0
                    ? completedSales.Average(s => s.Total)
                    : 0
            };

            return ServiceResult<DailySalesSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar resumen diario");
            return ServiceResult<DailySalesSummaryDto>.Failure("Error al generar resumen.");
        }
    }

    public async Task<ServiceResult<List<TopProductDto>>> GetTopProductsAsync(int companyId, int topCount = 10)
    {
        try
        {
            var sales = await _saleRepository.GetByDateRangeAsync(
                companyId, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

            var topProducts = sales
                .Where(s => s.Status == SaleStatus.Completed)
                .SelectMany(s => s.SaleDetails)
                .GroupBy(d => new { d.ProductId, d.ProductName, d.Barcode })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    Barcode = g.Key.Barcode,
                    TotalQuantity = g.Sum(d => (int)d.Quantity),
                    TotalRevenue = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(p => p.TotalQuantity)
                .Take(topCount)
                .ToList();

            return ServiceResult<List<TopProductDto>>.Success(topProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener top productos");
            return ServiceResult<List<TopProductDto>>.Failure("Error al cargar top productos.");
        }
    }

    public async Task<ServiceResult<List<LowStockProductDto>>> GetLowStockProductsAsync(int companyId)
    {
        try
        {
            var lowStock = await _productRepository.GetLowStockAsync(companyId, 0);
            var dtos = lowStock
                .Where(p => p.CurrentStock <= p.MinStock)
                .Select(p => new LowStockProductDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Barcode = p.Barcode,
                    CurrentStock = p.CurrentStock,
                    MinStock = p.MinStock
                })
                .OrderBy(p => p.CurrentStock)
                .ToList();

            return ServiceResult<List<LowStockProductDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos bajos en stock");
            return ServiceResult<List<LowStockProductDto>>.Failure("Error al cargar reporte.");
        }
    }

    // ========================================================================
    // Reportes Avanzados (Sub-fase 2.2)
    // ========================================================================

    public async Task<ServiceResult<List<SalesByPeriodDto>>> GetSalesByPeriodAsync(int companyId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var sales = await _saleRepository.GetByDateRangeAsync(companyId, startDate, endDate);
            var completedSales = sales.Where(s => s.Status == SaleStatus.Completed).ToList();

            var byPeriod = completedSales
                .GroupBy(s => s.CreatedAt.Date)
                .Select(g => new SalesByPeriodDto
                {
                    Date = g.Key,
                    TotalSales = g.Count(),
                    TotalAmount = g.Sum(s => s.Total),
                    ItemCount = g.Sum(s => s.SaleDetails.Sum(d => (int)d.Quantity))
                })
                .OrderBy(p => p.Date)
                .ToList();

            return ServiceResult<List<SalesByPeriodDto>>.Success(byPeriod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por período");
            return ServiceResult<List<SalesByPeriodDto>>.Failure("Error al cargar ventas por período.");
        }
    }

    public async Task<ServiceResult<ProfitMarginDto>> GetProfitMarginAsync(int companyId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var sales = await _saleRepository.GetByDateRangeAsync(companyId, startDate, endDate);
            var completedSales = sales.Where(s => s.Status == SaleStatus.Completed).ToList();

            decimal totalRevenue = completedSales.Sum(s => s.Total);
            decimal totalCost = completedSales
                .SelectMany(s => s.SaleDetails)
                .Sum(d => d.Cost * d.Quantity);
            decimal profit = totalRevenue - totalCost;
            decimal margin = totalRevenue > 0 ? Math.Round((profit / totalRevenue) * 100, 2) : 0;

            var result = new ProfitMarginDto
            {
                Period = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}",
                TotalRevenue = totalRevenue,
                TotalCost = totalCost,
                Profit = profit,
                Margin = margin
            };

            return ServiceResult<ProfitMarginDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular margen de ganancia");
            return ServiceResult<ProfitMarginDto>.Failure("Error al calcular margen de ganancia.");
        }
    }

    public async Task<ServiceResult<List<InventoryValuationDto>>> GetInventoryValuationAsync(int companyId)
    {
        try
        {
            var products = await _productRepository.GetByCompanyAsync(companyId);
            var activeWithStock = products
                .Where(p => p.IsActive && p.CurrentStock > 0)
                .Select(p => new InventoryValuationDto
                {
                    ProductName = p.Name,
                    Category = p.Category?.Name ?? "Sin categoría",
                    CurrentStock = p.CurrentStock,
                    UnitCost = p.Cost,
                    TotalValue = p.CurrentStock * p.Cost
                })
                .OrderByDescending(p => p.TotalValue)
                .ToList();

            return ServiceResult<List<InventoryValuationDto>>.Success(activeWithStock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener valorización de inventario");
            return ServiceResult<List<InventoryValuationDto>>.Failure("Error al cargar valorización de inventario.");
        }
    }

    public Task<ServiceResult<byte[]>> ExportReportToExcelAsync<T>(List<T> data, string reportName)
    {
        try
        {
            var bytes = _excelExportService.ExportToBytes(data);
            return Task.FromResult(ServiceResult<byte[]>.Success(bytes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar reporte a Excel");
            return Task.FromResult(ServiceResult<byte[]>.Failure("Error al exportar a Excel."));
        }
    }

    public Task<ServiceResult<byte[]>> ExportReportToPdfAsync<T>(List<T> data, string reportName)
    {
        try
        {
            var bytes = _pdfReportService.ExportToPdf(data, reportName);
            return Task.FromResult(ServiceResult<byte[]>.Success(bytes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar reporte a PDF");
            return Task.FromResult(ServiceResult<byte[]>.Failure("Error al exportar a PDF."));
        }
    }
}
