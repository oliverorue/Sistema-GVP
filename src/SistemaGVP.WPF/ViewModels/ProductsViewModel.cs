using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.WPF.Services;

namespace SistemaGVP.WPF.ViewModels;

public partial class ProductsViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IDialogService _dialogService;
    private readonly ICurrentUserService _currentUserService;
    private readonly BarcodeGeneratorService _barcodeGeneratorService;

    private const int DefaultPageSize = 50;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _items = new();

    [ObservableProperty]
    private ObservableCollection<CategoryDto> _categories = new();

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private ProductDto _editingProduct = new();

    [ObservableProperty]
    private CategoryDto? _selectedCategory;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private int _count;

    // === Paginación ===

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private bool _hasPreviousPage;

    [ObservableProperty]
    private bool _hasNextPage;

    [ObservableProperty]
    private string _paginationText = string.Empty;

    // === Barcode labels overlay ===

    [ObservableProperty]
    private bool _isShowingBarcodes;

    [ObservableProperty]
    private bool _hasBarcodesToShow;

    [ObservableProperty]
    private string _barcodeFilter = string.Empty;

    public ProductsViewModel(
        IProductService productService,
        ICategoryService categoryService,
        IDialogService dialogService,
        ICurrentUserService currentUserService,
        BarcodeGeneratorService barcodeGeneratorService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _productService = productService;
        _categoryService = categoryService;
        _dialogService = dialogService;
        _currentUserService = currentUserService;
        _barcodeGeneratorService = barcodeGeneratorService;
        ViewTitle = "Productos";
    }

    public override async Task LoadAsync()
    {
        await LoadPageAsync(1);
    }

    private async Task LoadPageAsync(int pageNumber)
    {
        await ExecuteSafeAsync(async () =>
        {
            _logger?.LogInformation("LoadPageAsync: Iniciando carga. PageNumber={PageNumber}, SearchTerm='{SearchTerm}'", pageNumber, SearchTerm);

            var companyId = _currentUserService.CompanyId;
            _logger?.LogInformation("LoadPageAsync: CompanyId={CompanyId}", companyId);

            var filter = new PaginationFilter(pageNumber, DefaultPageSize, SearchTerm);
            var result = await _productService.GetAllAsync(filter, companyId);

            _logger?.LogInformation("LoadPageAsync: Resultado obtenido. IsSuccess={IsSuccess}, Data is null={DataIsNull}, Message='{Message}'",
                result.IsSuccess, result.Data == null, result.Message);

            if (result.IsSuccess && result.Data != null)
            {
                var items = result.Data.Items ?? new List<ProductDto>();
                _logger?.LogInformation("LoadPageAsync: Items cargados. Count={ItemCount}, TotalCount={TotalCount}",
                    items.Count, result.Data.TotalCount);

                Items = new ObservableCollection<ProductDto>(items);
                Count = items.Count;
                IsEmpty = items.Count == 0 && result.Data.TotalCount == 0;

                // Actualizar estado de paginación
                CurrentPage = result.Data.PageNumber;
                TotalPages = result.Data.TotalPages;
                HasPreviousPage = result.Data.HasPrevious;
                HasNextPage = result.Data.HasNext;
                UpdatePaginationText();
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Message;
                IsEmpty = true;
                Count = 0;
            }

            var categoriesResult = await _categoryService.GetAllAsync(_currentUserService.CompanyId);
            if (categoriesResult.IsSuccess)
            {
                Categories = new ObservableCollection<CategoryDto>(categoriesResult.Data ?? new List<CategoryDto>());
            }
        }, "Cargar productos");
    }

    private void UpdatePaginationText()
    {
        if (TotalPages <= 1 && IsEmpty)
        {
            PaginationText = string.Empty;
        }
        else
        {
            PaginationText = $"Página {CurrentPage} de {TotalPages}";
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (!HasPreviousPage) return;
        await LoadPageAsync(CurrentPage - 1);
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!HasNextPage) return;
        await LoadPageAsync(CurrentPage + 1);
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadPageAsync(1);
    }

    partial void OnSearchTermChanged(string value)
    {
        // Fire-and-forget blindado con captura de excepciones via Serilog
        _ = LoadPageAsync(1).ContinueWith(t =>
        {
            if (t.IsFaulted && t.Exception != null)
                _logger?.LogError(t.Exception, "Error en OnSearchTermChanged al recargar página 1 con término: {SearchTerm}", value);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    [RelayCommand]
    private void NewProduct()
    {
        EditingProduct = new ProductDto { IsActive = true, CompanyId = _currentUserService.CompanyId };
        ClearError();
        IsEditing = true;
    }

    partial void OnSelectedCategoryChanged(CategoryDto? value)
    {
        if (value != null)
        {
            EditingProduct.CategoryId = value.Id;
            EditingProduct.CategoryName = value.Name;
        }
    }

    [RelayCommand]
    private void EditProduct(object? param)
    {
        if (param is not ProductDto product) return;
        EditingProduct = new ProductDto
        {
            Id = product.Id,
            CompanyId = product.CompanyId,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId,
            Name = product.Name,
            Description = product.Description,
            Barcode = product.Barcode,
            Sku = product.Sku,
            Price = product.Price,
            Cost = product.Cost,
            MinStock = product.MinStock,
            CurrentStock = product.CurrentStock,
            Unit = product.Unit,
            CategoryName = product.CategoryName,
            SupplierName = product.SupplierName,
            IsActive = product.IsActive
        };
        // Sync selected category from Categories list
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
        ClearError();
        IsEditing = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        EditingProduct = new ProductDto();
        SelectedCategory = null;
        ClearError();
    }

    /// <summary>
    /// Genera un código de barras EAN-13 automático para el producto en edición.
    /// Si el producto ya tiene ID (existente), usa GenerateBarcode(id).
    /// Si es nuevo, genera uno aleatorio con GenerateRandomBarcode().
    /// </summary>
    [RelayCommand]
    private void GenerateBarcode()
    {
        if (EditingProduct.Id > 0)
        {
            EditingProduct.Barcode = _barcodeGeneratorService.GenerateBarcode(EditingProduct.Id);
        }
        else
        {
            EditingProduct.Barcode = _barcodeGeneratorService.GenerateRandomBarcode();
        }
    }

    /// <summary>
    /// Genera códigos de barras para TODOS los productos que aún no tienen uno.
    /// Útil para inicializar el sistema con productos existentes.
    /// </summary>
    [RelayCommand]
    private async Task GenerateBarcodesForAllAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var filter = new PaginationFilter(1, 500, string.Empty);
            var result = await _productService.GetAllAsync(filter, _currentUserService.CompanyId);
            if (!result.IsSuccess || result.Data?.Items == null)
            {
                await _dialogService.ShowWarningAsync("No se pudieron cargar los productos.");
                return;
            }

            var productsWithoutBarcode = result.Data.Items
                .Where(p => string.IsNullOrWhiteSpace(p.Barcode) || !BarcodeGeneratorService.IsValidBarcode(p.Barcode))
                .ToList();

            if (productsWithoutBarcode.Count == 0)
            {
                await _dialogService.ShowInfoAsync("Todos los productos ya tienen un código de barras válido.");
                return;
            }

            var confirm = await _dialogService.ShowConfirmAsync(
                $"Se generarán códigos de barras para {productsWithoutBarcode.Count} producto(s) sin código. ¿Continuar?",
                "Generar códigos masivos");

            if (!confirm) return;

            int generated = 0;
            foreach (var product in productsWithoutBarcode)
            {
                product.Barcode = _barcodeGeneratorService.GenerateBarcode(product.Id);
                var updateResult = await _productService.UpdateAsync(product);
                if (updateResult.IsSuccess)
                    generated++;
            }

            await LoadAsync();
            await _dialogService.ShowInfoAsync(
                $"✅ Códigos generados correctamente para {generated} de {productsWithoutBarcode.Count} productos.",
                "Generación completa");
        }, "Generar códigos masivos");
    }

    [RelayCommand]
    private void ShowBarcodeLabels()
    {
        BarcodeFilter = string.Empty;
        HasBarcodesToShow = Items.Any(p => !string.IsNullOrWhiteSpace(p.Barcode));
        IsShowingBarcodes = true;
    }

    [RelayCommand]
    private void CloseBarcodeLabels()
    {
        IsShowingBarcodes = false;
    }

    [RelayCommand]
    private async Task SaveProductAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            if (EditingProduct.Id == 0)
            {
                var result = await _productService.CreateAsync(EditingProduct);
                if (!result.IsSuccess)
                {
                    HasError = true;
                    ErrorMessage = result.Message;
                    return;
                }
            }
            else
            {
                var result = await _productService.UpdateAsync(EditingProduct);
                if (!result.IsSuccess)
                {
                    HasError = true;
                    ErrorMessage = result.Message;
                    return;
                }
            }

            IsEditing = false;
            EditingProduct = new ProductDto();
            SelectedCategory = null;
            await LoadAsync();
            await SetTemporaryStatusAsync("Producto guardado correctamente");
        }, "Guardar producto");
    }

    [RelayCommand]
    private async Task DeleteProductAsync(object? param)
    {
        if (param is not ProductDto product) return;

        var confirm = await _dialogService.ShowConfirmAsync(
            $"¿Eliminar el producto '{product.Name}'?", "Confirmar Eliminación");
        if (!confirm) return;

        await ExecuteSafeAsync(async () =>
        {
            var result = await _productService.DeleteAsync(product.Id);
            if (result.IsSuccess)
            {
                await LoadAsync();
                await SetTemporaryStatusAsync("Producto eliminado correctamente");
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Message;
            }
        }, "Eliminar producto");
    }
}
