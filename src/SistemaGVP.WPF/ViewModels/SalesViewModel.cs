using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Enums;
using SistemaGVP.WPF.Services;
using System;

namespace SistemaGVP.WPF.ViewModels;

/// <summary>
/// Item de carrito para visualización en la UI.
/// Wrappeo de CreateSaleDetailDto que agrega ProductName para mostrar en DataGrid.
/// </summary>
public class CartItemDisplay
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;

    /// <summary>Convierte a CreateSaleDetailDto para enviar a la API.</summary>
    public CreateSaleDetailDto ToDto() => new()
    {
        ProductId = ProductId,
        Quantity = Quantity,
        UnitPrice = UnitPrice,
        Discount = Discount
    };
}

/// <summary>
/// ViewModel del Punto de Venta (SalesView).
/// Gestiona carrito de compras, búsqueda de productos y clientes,
/// cálculo de totales, procesamiento de pagos y ventas en espera,
/// e integración con escáner móvil QR/código de barras.
/// </summary>
public partial class SalesViewModel : BaseViewModel
{
    private readonly ISaleService _saleService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDialogService _dialogService;
    private readonly BarcodeHttpServer _barcodeHttpServer;
    private readonly QrCodeService _qrCodeService;

    // Variable para evitar leak de CollectionChanged (A5)
    private ObservableCollection<CartItemDisplay>? _previousCartItems;

    // ───────────────────────────── Carrito ─────────────────────────────

    /// <summary>Items actuales en el carrito de compra.</summary>
    [ObservableProperty]
    private ObservableCollection<CartItemDisplay> _cartItems = new();

    /// <summary>Item seleccionado actualmente en el DataGrid del carrito.</summary>
    [ObservableProperty]
    private CartItemDisplay? _selectedCartItem;

    /// <summary>Indica si el carrito tiene items (para UI).</summary>
    [ObservableProperty]
    private bool _hasCartItems;

    /// <summary>Indica si hay resultados de búsqueda (para UI).</summary>
    [ObservableProperty]
    private bool _hasSearchResults;

    // ───────────────────────── Búsqueda productos ─────────────────────

    /// <summary>Término de búsqueda (código de barras o nombre de producto).</summary>
    [ObservableProperty]
    private string _searchTerm = string.Empty;

    /// <summary>Resultados de la búsqueda de productos.</summary>
    [ObservableProperty]
    private ObservableCollection<ProductDto> _searchResults = new();

    /// <summary>Indica si se está realizando una búsqueda.</summary>
    [ObservableProperty]
    private bool _isSearching;

    // ───────────────────────────── Cliente ─────────────────────────────

    /// <summary>Cliente seleccionado para la venta actual.</summary>
    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    /// <summary>Término de búsqueda de clientes.</summary>
    [ObservableProperty]
    private string _customerSearchTerm = string.Empty;

    /// <summary>Resultados de la búsqueda de clientes.</summary>
    [ObservableProperty]
    private ObservableCollection<CustomerDto> _customerSearchResults = new();

    /// <summary>Indica si el panel de búsqueda de clientes está abierto.</summary>
    [ObservableProperty]
    private bool _isCustomerPanelOpen;

    // ──────────────────── Totales y pago ────────────────────

    /// <summary>Subtotal de la venta (suma de Quantity * UnitPrice).</summary>
    [ObservableProperty]
    private decimal _subtotal;

    /// <summary>Monto del IVA calculado (Subtotal * TaxRate).</summary>
    [ObservableProperty]
    private decimal _taxAmount;

    /// <summary>Total de la venta (Subtotal + TaxAmount).</summary>
    [ObservableProperty]
    private decimal _total;

    /// <summary>Tasa de IVA (10% por defecto).</summary>
    [ObservableProperty]
    private decimal _taxRate = 0.10m;

    /// <summary>Cantidad total de unidades en el carrito.</summary>
    [ObservableProperty]
    private int _totalItems;

    /// <summary>Método de pago seleccionado.</summary>
    [ObservableProperty]
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;

    /// <summary>Lista de métodos de pago disponibles.</summary>
    [ObservableProperty]
    private List<PaymentMethod> _paymentMethods = new();

    /// <summary>Monto recibido del cliente.</summary>
    [ObservableProperty]
    private decimal _amountPaid;

    /// <summary>Vuelto a devolver al cliente (AmountPaid - Total).</summary>
    [ObservableProperty]
    private decimal _changeAmount;

    /// <summary>Indica si el panel de pago está abierto.</summary>
    [ObservableProperty]
    private bool _isPaymentPanelOpen;

    /// <summary>Indica si hay cambio positivo que mostrar.</summary>
    [ObservableProperty]
    private bool _hasChange;

    // ────────────────────── Ventas en espera ──────────────────────

    /// <summary>Lista de ventas en espera.</summary>
    [ObservableProperty]
    private ObservableCollection<HeldSaleDto> _heldSales = new();

    /// <summary>Indica si el panel de ventas en espera está abierto.</summary>
    [ObservableProperty]
    private bool _isHeldSalesPanelOpen;

    /// <summary>Indica si existen ventas en espera.</summary>
    [ObservableProperty]
    private bool _hasHeldSales;

    // ────────────────────── Escáner Móvil ──────────────────────

    /// <summary>Visibilidad del overlay del escáner móvil.</summary>
    [ObservableProperty]
    private bool _isScannerPanelOpen;

    /// <summary>Indica si el servidor HTTP del escáner está corriendo.</summary>
    [ObservableProperty]
    private bool _isScannerRunning;

    /// <summary>URL del servidor del escáner (ej: https://192.168.1.5:5180).</summary>
    [ObservableProperty]
    private string _scannerUrl = string.Empty;

    /// <summary>Imagen QR generada con la URL del servidor del escáner.</summary>
    [ObservableProperty]
    private BitmapImage? _scannerQrCode;

    /// <summary>Colección de códigos escaneados recientemente (máx 10).</summary>
    [ObservableProperty]
    private ObservableCollection<string> _scannerRecentScans = new();

    /// <summary>Mensaje de estado del escáner móvil.</summary>
    [ObservableProperty]
    private string _scannerStatusMessage = string.Empty;

    // ────────────────────── Constructor ──────────────────────

    public SalesViewModel(
        ISaleService saleService,
        IProductService productService,
        ICustomerService customerService,
        ICurrentUserService currentUserService,
        IDialogService dialogService,
        BarcodeHttpServer barcodeHttpServer,
        QrCodeService qrCodeService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _saleService = saleService;
        _productService = productService;
        _customerService = customerService;
        _currentUserService = currentUserService;
        _dialogService = dialogService;
        _barcodeHttpServer = barcodeHttpServer;
        _qrCodeService = qrCodeService;

        ViewTitle = "Punto de Venta";

        // Suscribirse al evento de código escaneado
        _barcodeHttpServer.BarcodeScanned += OnBarcodeScanned;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _barcodeHttpServer.BarcodeScanned -= OnBarcodeScanned;
        }
        base.Dispose(disposing);
    }

    // ────────────────────── LoadAsync ──────────────────────

    /// <summary>
    /// Inicializa el ViewModel: carga métodos de pago, tasa de IVA y ventas en espera.
    /// El escáner no se inicia automáticamente; se activa bajo demanda.
    /// </summary>
    public override async Task LoadAsync()
    {
        PaymentMethods = Enum.GetValues<PaymentMethod>().ToList();
        TaxRate = 0.10m;
        await LoadHeldSalesAsync();
    }

    // ────────────────── Búsqueda de productos ──────────────────

    /// <summary>
    /// Busca productos por código de barras o nombre usando IProductService.
    /// Primero intenta búsqueda exacta por barcode, luego búsqueda paginada por término.
    /// </summary>
    [RelayCommand]
    private async Task SearchProductsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            SearchResults.Clear();
            HasSearchResults = false;
            return;
        }

        await ExecuteSafeAsync(async () =>
        {
            IsSearching = true;
            var companyId = _currentUserService.CompanyId;
            var term = SearchTerm.Trim();
            var results = new List<ProductDto>();

            // 1. Búsqueda exacta por código de barras
            var barcodeResult = await _productService.GetByBarcodeAsync(term, companyId);
            if (barcodeResult.IsSuccess && barcodeResult.Data != null)
            {
                results.Add(barcodeResult.Data);
            }

            // 2. Búsqueda general paginada por término (nombre, SKU, etc.)
            var filter = new PaginationFilter(1, 50, term);
            var allResult = await _productService.GetAllAsync(filter, companyId);
            if (allResult.IsSuccess && allResult.Data?.Items != null)
            {
                foreach (var p in allResult.Data.Items)
                {
                    if (!results.Any(r => r.Id == p.Id))
                        results.Add(p);
                }
            }

            // Solo productos activos
            results = results.Where(p => p.IsActive).ToList();
            SearchResults = new ObservableCollection<ProductDto>(results);
            HasSearchResults = SearchResults.Count > 0;
            IsSearching = false;
        }, "BuscarProductos");
    }

    /// <summary>Limpia los resultados cuando se vacía el término de búsqueda.</summary>
    partial void OnSearchTermChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SearchResults.Clear();
            HasSearchResults = false;
        }
    }

    // ─────────────────── Agregar al carrito ───────────────────

    /// <summary>
    /// Agrega un producto al carrito. Si ya existe, incrementa la cantidad.
    /// Valida que no exceda el stock disponible.
    /// </summary>
    [RelayCommand]
    private void AddToCart(object? param)
    {
        if (param is not ProductDto product) return;

        // Validar stock
        if (product.CurrentStock <= 0)
        {
            StatusMessage = $"⚠️ {product.Name} no tiene stock disponible";
            return;
        }

        // Buscar si el producto ya está en el carrito
        var existingItem = CartItems.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            if (existingItem.Quantity >= product.CurrentStock)
            {
                StatusMessage = $"⚠️ Stock máximo alcanzado para {product.Name} ({product.CurrentStock})";
                return;
            }
            existingItem.Quantity++;
        }
        else
        {
            CartItems.Add(new CartItemDisplay
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = 1,
                UnitPrice = product.Price,
                Discount = 0
            });
        }

        RecalculateTotals();
        UpdateCartFlags();
        StatusMessage = $"✅ {product.Name} agregado al carrito";
    }

    // ─────────────────── Remover del carrito ───────────────────

    [RelayCommand]
    private void RemoveFromCart(object? param)
    {
        if (param is not CartItemDisplay item) return;
        CartItems.Remove(item);
        RecalculateTotals();
        UpdateCartFlags();
    }

    // ─────────────────── Ajustar cantidades ───────────────────

    [RelayCommand]
    private void IncreaseQuantity(object? param)
    {
        if (param is not CartItemDisplay item) return;
        item.Quantity++;
        RecalculateTotals();
    }

    [RelayCommand]
    private void DecreaseQuantity(object? param)
    {
        if (param is not CartItemDisplay item) return;
        if (item.Quantity <= 1)
        {
            CartItems.Remove(item);
            UpdateCartFlags();
        }
        else
        {
            item.Quantity--;
        }
        RecalculateTotals();
    }

    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        SelectedCustomer = null;
        AmountPaid = 0;
        RecalculateTotals();
        UpdateCartFlags();
        StatusMessage = "🔄 Carrito vaciado";
    }

    // ─────────────────── Cliente ───────────────────

    [RelayCommand]
    private void OpenCustomerPanel()
    {
        CustomerSearchTerm = string.Empty;
        CustomerSearchResults.Clear();
        IsCustomerPanelOpen = true;
    }

    [RelayCommand]
    private void CloseCustomerPanel()
    {
        IsCustomerPanelOpen = false;
    }

    [RelayCommand]
    private async Task SearchCustomersAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerSearchTerm))
        {
            CustomerSearchResults.Clear();
            return;
        }

        await ExecuteSafeAsync(async () =>
        {
            var result = await _customerService.SearchAsync(
                CustomerSearchTerm.Trim(), _currentUserService.CompanyId);

            if (result.IsSuccess && result.Data != null)
            {
                CustomerSearchResults = new ObservableCollection<CustomerDto>(result.Data);
            }
        }, "BuscarClientes");
    }

    [RelayCommand]
    private void SelectCustomer(object? param)
    {
        if (param is not CustomerDto customer) return;
        SelectedCustomer = customer;
        IsCustomerPanelOpen = false;
        StatusMessage = $"👤 Cliente: {customer.Name}";
    }

    // ─────────────────── Pago ───────────────────

    [RelayCommand]
    private void OpenPaymentPanel()
    {
        if (CartItems.Count == 0)
        {
            StatusMessage = "⚠️ Agregue productos al carrito antes de cobrar";
            return;
        }

        if (SelectedCustomer == null)
        {
            StatusMessage = "⚠️ Seleccione un cliente antes de cobrar";
            return;
        }

        AmountPaid = 0;
        ChangeAmount = 0;
        HasChange = false;
        IsPaymentPanelOpen = true;
    }

    [RelayCommand]
    private void ClosePaymentPanel()
    {
        IsPaymentPanelOpen = false;
    }

    /// <summary>Recalcula el cambio cuando cambia el monto recibido.</summary>
    partial void OnAmountPaidChanged(decimal value)
    {
        var change = Math.Max(0, value - Total);
        ChangeAmount = change;
        HasChange = change > 0;
    }

    // ─────────────────── Completar venta ───────────────────

    [RelayCommand]
    private async Task CompleteSaleAsync()
    {
        if (CartItems.Count == 0)
        {
            await _dialogService.ShowWarningAsync("No hay productos en el carrito.", "Venta inválida");
            return;
        }

        if (SelectedCustomer == null)
        {
            await _dialogService.ShowWarningAsync("Debe seleccionar un cliente.", "Venta inválida");
            return;
        }

        if (AmountPaid < Total)
        {
            await _dialogService.ShowWarningAsync(
                $"El monto recibido (Gs. {AmountPaid:N0}) es menor al total (Gs. {Total:N0}).",
                "Monto insuficiente");
            return;
        }

        var confirm = await _dialogService.ShowConfirmAsync(
            $"Confirmar venta por Gs. {Total:N0} a {SelectedCustomer.Name}?",
            "Confirmar Venta");

        if (!confirm) return;

        await ExecuteSafeAsync(async () =>
        {
            var dto = new CreateSaleDto
            {
                CompanyId = _currentUserService.CompanyId,
                UserId = _currentUserService.UserId,
                CustomerId = SelectedCustomer.Id,
                PaymentMethod = SelectedPaymentMethod.ToString(),
                CashAmount = AmountPaid,
                Items = CartItems.Select(i => i.ToDto()).ToList()
            };

            var result = await _saleService.CreateSaleAsync(dto);

            if (result.IsSuccess)
            {
                await SetTemporaryStatusAsync($"✅ Venta #{result.Data?.Id} completada — Gs. {Total:N0}");
                await ResetSaleAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Message, "Error al procesar la venta");
            }
        }, "CompletarVenta");
    }

    // ─────────────────── Ventas en espera ───────────────────

    [RelayCommand]
    private async Task HoldSaleAsync()
    {
        if (CartItems.Count == 0)
        {
            await _dialogService.ShowWarningAsync("No hay productos en el carrito.", "Venta en espera");
            return;
        }

        var dto = new CreateSaleDto
        {
            CompanyId = _currentUserService.CompanyId,
            UserId = _currentUserService.UserId,
            CustomerId = SelectedCustomer?.Id,
            PaymentMethod = SelectedPaymentMethod.ToString(),
            CashAmount = AmountPaid,
            Items = CartItems.Select(i => i.ToDto()).ToList()
        };

        await ExecuteSafeAsync(async () =>
        {
            var result = await _saleService.HoldSaleAsync(
                dto, _currentUserService.CompanyId, _currentUserService.UserId);

            if (result.IsSuccess)
            {
                await SetTemporaryStatusAsync("⏸️ Venta puesta en espera");
                await ResetSaleAsync();
                await LoadHeldSalesAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Message, "Error al poner en espera");
            }
        }, "PonerEnEspera");
    }

    [RelayCommand]
    private async Task OpenHeldSalesPanel()
    {
        await LoadHeldSalesAsync();
        IsHeldSalesPanelOpen = true;
    }

    [RelayCommand]
    private void CloseHeldSalesPanel()
    {
        IsHeldSalesPanelOpen = false;
    }

    [RelayCommand]
    private async Task ResumeSaleAsync(object? param)
    {
        if (param is not HeldSaleDto heldSale) return;

        await ExecuteSafeAsync(async () =>
        {
            var result = await _saleService.ResumeSaleAsync(heldSale.Id, _currentUserService.CompanyId);

            if (result.IsSuccess && result.Data != null)
            {
                // Reconstruir carrito desde los detalles de la venta en espera
                CartItems.Clear();
                foreach (var detail in result.Data.Items)
                {
                    CartItems.Add(new CartItemDisplay
                    {
                        ProductId = detail.ProductId,
                        ProductName = detail.ProductName,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        Discount = detail.Discount
                    });
                }

                // HeldSaleDto solo expone CustomerName, no CustomerId,
                // por lo que el cliente no puede restaurarse automáticamente.
                // El usuario deberá seleccionarlo manualmente.

                RecalculateTotals();
                UpdateCartFlags();
                IsHeldSalesPanelOpen = false;

                // Eliminar la venta en espera (ya se está reanudando)
                await _saleService.RemoveHeldSaleAsync(heldSale.Id, _currentUserService.CompanyId);
                await LoadHeldSalesAsync();

                await SetTemporaryStatusAsync("🔄 Venta reanudada");
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Message, "Error al reanudar venta");
            }
        }, "ReanudarVenta");
    }

    [RelayCommand]
    private async Task DeleteHeldSaleAsync(object? param)
    {
        if (param is not HeldSaleDto heldSale) return;

        var confirm = await _dialogService.ShowConfirmAsync(
            $"¿Eliminar la venta en espera de {heldSale.CustomerName ?? "Cliente desconocido"} por Gs. {heldSale.SaleTotal:N0}?",
            "Eliminar venta en espera");

        if (!confirm) return;

        await ExecuteSafeAsync(async () =>
        {
            var result = await _saleService.RemoveHeldSaleAsync(heldSale.Id, _currentUserService.CompanyId);
            if (result.IsSuccess)
            {
                await LoadHeldSalesAsync();
                await SetTemporaryStatusAsync("🗑️ Venta en espera eliminada");
            }
            else
            {
                await _dialogService.ShowErrorAsync(result.Message, "Error al eliminar");
            }
        }, "EliminarVentaEspera");
    }

    // ─────────────────── Escáner Móvil ───────────────────

    /// <summary>
    /// Abre o cierra el panel del escáner móvil.
    /// Al abrir, inicia el servidor HTTP (Kestrel) y genera el QR con la URL.
    /// Al cerrar, solo oculta el panel sin detener el servidor.
    /// </summary>
    [RelayCommand]
    private async Task ToggleScannerPanelAsync()
    {
        if (IsScannerPanelOpen)
        {
            // Cerrar panel: solo ocultar, no detener servidor
            IsScannerPanelOpen = false;
            return;
        }

        // Abrir panel
        IsScannerPanelOpen = true;
        ScannerStatusMessage = "Iniciando escáner...";

        var started = await _barcodeHttpServer.StartAsync();
        if (started)
        {
            IsScannerRunning = true;
            var url = _barcodeHttpServer.ServerUrl;
            ScannerUrl = url;
            ScannerQrCode = _qrCodeService.GenerateQrCode(url);
            ScannerStatusMessage = "✅ Escáner listo. Escaneá el QR con tu celular.";
        }
        else
        {
            IsScannerRunning = false;
            ScannerUrl = string.Empty;
            ScannerQrCode = null;
            ScannerStatusMessage = "❌ No se pudo iniciar el servidor. Revisá los logs.";
        }
    }

    /// <summary>
    /// Detiene el servidor del escáner y cierra el panel.
    /// </summary>
    [RelayCommand]
    private async Task StopScanner()
    {
        await _barcodeHttpServer.StopAsync();
        IsScannerRunning = false;
        IsScannerPanelOpen = false;
        ScannerUrl = string.Empty;
        ScannerQrCode = null;
        ScannerRecentScans.Clear();
        ScannerStatusMessage = "⏹️ Escáner detenido";
    }

    /// <summary>
    /// Handler del evento BarcodeScanned del servidor HTTP.
    /// Busca el producto por código de barras y lo agrega al carrito.
    /// Es async void forzoso por ser event handler. Blindado con try/catch.
    /// </summary>
    private async void OnBarcodeScanned(object? sender, string barcode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(barcode)) return;

            var companyId = _currentUserService.CompanyId;

            // Buscar producto por código de barras
            var result = await _productService.GetByBarcodeAsync(barcode.Trim(), companyId);

            // Agregar a ScannerRecentScans (máx 10, insertar al inicio)
            // Debe ejecutarse en el hilo UI porque ScannerRecentScans es un ObservableCollection
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (ScannerRecentScans.Count >= 10)
                    ScannerRecentScans.RemoveAt(ScannerRecentScans.Count - 1);
                ScannerRecentScans.Insert(0, barcode.Trim());
            });

            if (result.IsSuccess && result.Data != null)
            {
                // Agregar al carrito (AddToCart espera object?, usamos dispatcher para thread-safety)
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AddToCart(result.Data);
                    ScannerStatusMessage = $"✅ Producto agregado: {result.Data.Name}";
                });
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ScannerStatusMessage = $"⚠️ Producto no encontrado: {barcode.Trim()}";
                });
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error en OnBarcodeScanned para código: {Barcode}", barcode);
        }
    }

    // ─────────────────── Métodos privados ───────────────────

    /// <summary>Recalcula Subtotal, TaxAmount, Total y TotalItems desde CartItems.</summary>
    private void RecalculateTotals()
    {
        Subtotal = CartItems.Sum(i => i.Quantity * i.UnitPrice);
        TaxAmount = Subtotal * TaxRate;
        Total = Subtotal + TaxAmount;
        TotalItems = (int)CartItems.Sum(i => i.Quantity);
    }

    /// <summary>Actualiza flags booleanos de UI basados en el estado del carrito.</summary>
    private void UpdateCartFlags()
    {
        HasCartItems = CartItems.Count > 0;
    }

    /// <summary>Carga la lista de ventas en espera desde el servicio.</summary>
    private async Task LoadHeldSalesAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var result = await _saleService.GetHeldSalesAsync(_currentUserService.CompanyId);
            if (result.IsSuccess && result.Data != null)
            {
                HeldSales = new ObservableCollection<HeldSaleDto>(result.Data);
                HasHeldSales = HeldSales.Count > 0;
            }
        }, "CargarVentasEspera");
    }

    /// <summary>
    /// Reinicia el estado de la venta actual después de completar o poner en espera.
    /// No detiene el escáner (puede seguir activo entre ventas).
    /// </summary>
    private async Task ResetSaleAsync()
    {
        CartItems.Clear();
        SelectedCustomer = null;
        AmountPaid = 0;
        ChangeAmount = 0;
        HasChange = false;
        SearchTerm = string.Empty;
        SearchResults.Clear();
        HasSearchResults = false;
        IsPaymentPanelOpen = false;
        RecalculateTotals();
        UpdateCartFlags();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Suscripción a cambios en la colección del carrito para recalcular automáticamente.
    /// Desuscribe la colección anterior para evitar memory leaks (A5).
    /// </summary>
    partial void OnCartItemsChanged(ObservableCollection<CartItemDisplay> value)
    {
        // Desuscribir la colección anterior
        if (_previousCartItems != null)
        {
            _previousCartItems.CollectionChanged -= OnCartCollectionChanged;
        }

        // Suscribir la nueva colección
        value.CollectionChanged += OnCartCollectionChanged;
        _previousCartItems = value;
    }

    private void OnCartCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RecalculateTotals();
        UpdateCartFlags();
    }
}
