using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.WPF.ViewModels;

public partial class InventoryViewModel : BaseViewModel
{
    private readonly IInventoryService _inventoryService;
    private readonly ICurrentUserService _currentUserService;

    private const int PageSize = 50;

    // === Stock bajo ===

    [ObservableProperty]
    private ObservableCollection<ProductDto> _lowStockItems = new();

    [ObservableProperty]
    private bool _hasLowStock;

    [ObservableProperty]
    private string _lowStockCountText = string.Empty;

    // === Movimientos recientes ===

    [ObservableProperty]
    private ObservableCollection<InventoryMovementDto> _recentMovements = new();

    [ObservableProperty]
    private bool _hasMovements;

    // === Búsqueda ===

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    // === Panel de ajuste ===

    [ObservableProperty]
    private bool _isAdjustOpen;

    [ObservableProperty]
    private ProductDto? _selectedProduct;

    [ObservableProperty]
    private int _adjustmentQuantity;

    [ObservableProperty]
    private string _adjustmentReason = string.Empty;

    [ObservableProperty]
    private string _adjustmentType = "Entrada";

    [ObservableProperty]
    private List<string> _adjustmentTypes = new() { "Entrada", "Salida" };

    // === Paginación de movimientos ===

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private string _pageInfo = string.Empty;

    [ObservableProperty]
    private bool _hasPreviousPage;

    [ObservableProperty]
    private bool _hasNextPage;

    // === Estado de carga ===

    [ObservableProperty]
    private bool _isLoading;

    public InventoryViewModel(
        IInventoryService inventoryService,
        ICurrentUserService currentUserService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _inventoryService = inventoryService;
        _currentUserService = currentUserService;
        ViewTitle = "Inventario";
    }

    public override async Task LoadAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            IsLoading = true;
            await LoadLowStockAsync();
            await LoadMovementsAsync(1);
            IsLoading = false;
        }, "Cargar inventario");
    }

    // --- Carga de stock bajo ---

    private async Task LoadLowStockAsync()
    {
        var companyId = _currentUserService.CompanyId;
        var result = await _inventoryService.GetLowStockProductsAsync(companyId);

        if (result.IsSuccess && result.Data != null)
        {
            LowStockItems = new ObservableCollection<ProductDto>(result.Data);
            HasLowStock = result.Data.Count > 0;
            LowStockCountText = result.Data.Count > 0
                ? $"⚠️ {result.Data.Count} producto(s) con stock bajo"
                : "✅ Todos los productos tienen stock suficiente";
        }
        else
        {
            HasLowStock = false;
            LowStockCountText = "No se pudo verificar el stock";
        }
    }

    // --- Carga de movimientos ---

    private async Task LoadMovementsAsync(int page)
    {
        var companyId = _currentUserService.CompanyId;
        var result = await _inventoryService.GetRecentMovementsAsync(companyId, PageSize);

        if (result.IsSuccess && result.Data != null)
        {
            RecentMovements = new ObservableCollection<InventoryMovementDto>(result.Data);
            HasMovements = result.Data.Count > 0;

            // Simulación de paginación: el backend devuelve hasta PageSize items
            CurrentPage = 1;
            TotalPages = 1;
            HasPreviousPage = false;
            HasNextPage = false;
            UpdatePageInfo();
        }
        else
        {
            HasMovements = false;
            RecentMovements.Clear();
            UpdatePageInfo();
        }
    }

    private void UpdatePageInfo()
    {
        PageInfo = HasMovements
            ? $"Página {CurrentPage} de {TotalPages}"
            : "Sin movimientos";
    }

    // --- Comandos ---

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadAsync();
    }

    /// <summary>
    /// Dispara búsqueda al cambiar el término (debounced por el binding).
    /// </summary>
    partial void OnSearchTermChanged(string value)
    {
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
        await SetTemporaryStatusAsync("Inventario actualizado");
    }

    [RelayCommand]
    private void OpenAdjust(object? param)
    {
        if (param is not ProductDto product) return;

        SelectedProduct = product;
        AdjustmentQuantity = 0;
        AdjustmentReason = string.Empty;
        AdjustmentType = "Entrada";
        IsAdjustOpen = true;
    }

    [RelayCommand]
    private void CloseAdjust()
    {
        IsAdjustOpen = false;
        SelectedProduct = null;
        AdjustmentQuantity = 0;
        AdjustmentReason = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAdjustmentAsync()
    {
        if (SelectedProduct == null) return;

        if (AdjustmentQuantity <= 0)
        {
            HasError = true;
            ErrorMessage = "La cantidad debe ser mayor a cero.";
            return;
        }

        if (string.IsNullOrWhiteSpace(AdjustmentReason))
        {
            HasError = true;
            ErrorMessage = "Debe ingresar un motivo para el ajuste.";
            return;
        }

        await ExecuteSafeAsync(async () =>
        {
            var dto = new CreateInventoryMovementDto
            {
                ProductId = SelectedProduct.Id,
                UserId = _currentUserService.UserId,
                CompanyId = _currentUserService.CompanyId,
                Type = AdjustmentType,
                Quantity = AdjustmentQuantity,
                Reason = AdjustmentReason
            };

            var result = await _inventoryService.AdjustStockAsync(dto);

            if (result.IsSuccess)
            {
                CloseAdjust();
                await LoadAsync();
                await SetTemporaryStatusAsync("✅ Ajuste de stock guardado correctamente");
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Message;
            }
        }, "Guardar ajuste de stock");
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!HasNextPage) return;
        await LoadMovementsAsync(CurrentPage + 1);
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (!HasPreviousPage) return;
        await LoadMovementsAsync(CurrentPage - 1);
    }
}
