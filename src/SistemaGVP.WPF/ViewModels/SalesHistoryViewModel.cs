using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.WPF.ViewModels;

public partial class SalesHistoryViewModel : BaseViewModel
{
    private readonly ISaleService _saleService;
    private readonly ICurrentUserService _currentUserService;

    private const int PageSize = 20;

    [ObservableProperty]
    private ObservableCollection<SaleHistoryDto> _items = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private string? _searchTerm;

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private SaleHistoryDto? _selectedSale;

    [ObservableProperty]
    private bool _isDetailOpen;

    [ObservableProperty]
    private ObservableCollection<SaleDetailDto> _saleDetails = new();

    [ObservableProperty]
    private bool _hasPreviousPage;

    [ObservableProperty]
    private bool _hasNextPage;

    [ObservableProperty]
    private string _pageInfo = string.Empty;

    public SalesHistoryViewModel(
        ISaleService saleService,
        ICurrentUserService currentUserService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _saleService = saleService;
        _currentUserService = currentUserService;
        ViewTitle = "Historial de Ventas";
    }

    public override async Task LoadAsync()
    {
        await LoadPageAsync(1);
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadPageAsync(1);
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchTerm = null;
        StartDate = null;
        EndDate = null;
        await LoadPageAsync(1);
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (HasNextPage)
            await LoadPageAsync(CurrentPage + 1);
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (HasPreviousPage)
            await LoadPageAsync(CurrentPage - 1);
    }

    [RelayCommand]
    private async Task ViewDetailAsync(object? param)
    {
        if (param is not SaleHistoryDto sale) return;

        await ExecuteSafeAsync(async () =>
        {
            IsLoading = true;
            var result = await _saleService.GetByIdAsync(sale.Id);

            if (result.IsSuccess && result.Data is not null)
            {
                SelectedSale = sale;
                SaleDetails = new ObservableCollection<SaleDetailDto>(result.Data.Items);
                IsDetailOpen = true;
            }
            else
            {
                await SetTemporaryStatusAsync($"Error al cargar detalle: {result.Message}", 3000);
            }
        }, "Cargar detalle de venta");

        IsLoading = false;
    }

    [RelayCommand]
    private void CloseDetail()
    {
        IsDetailOpen = false;
        SelectedSale = null;
        SaleDetails.Clear();
    }

    private async Task LoadPageAsync(int page)
    {
        await ExecuteSafeAsync(async () =>
        {
            IsLoading = true;
            var companyId = _currentUserService.CompanyId;
            var result = await _saleService.GetSalesHistoryAsync(
                companyId, SearchTerm, StartDate, EndDate, null, page, PageSize);

            if (result.IsSuccess && result.Data is not null)
            {
                Items = new ObservableCollection<SaleHistoryDto>(result.Data.Items);
                TotalCount = result.Data.TotalCount;
                CurrentPage = result.Data.PageNumber;
                TotalPages = result.Data.TotalPages;
                HasPreviousPage = CurrentPage > 1;
                HasNextPage = CurrentPage < TotalPages;
                PageInfo = $"Página {CurrentPage} de {TotalPages} — {TotalCount} ventas";
            }
            else
            {
                await SetTemporaryStatusAsync($"Error: {result.Message}", 3000);
            }
        }, "Cargar historial de ventas");

        IsLoading = false;
    }
}
