using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.WPF.ViewModels;

public partial class AuditLogViewModel : BaseViewModel
{
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private ObservableCollection<AuditLogDto> _items = new();

    [ObservableProperty]
    private string? _actionFilter;

    [ObservableProperty]
    private string? _entityFilter;

    [ObservableProperty]
    private DateTime? _startDate = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime? _endDate = DateTime.Today;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private string _paginationText = "Página 1 de 1";

    [ObservableProperty]
    private bool _hasPreviousPage;

    [ObservableProperty]
    private bool _hasNextPage;

    [ObservableProperty]
    private AuditLogDto? _selectedLog;

    [ObservableProperty]
    private bool _isViewingDetail;

    private const int PageSize = 50;

    // Lista de acciones posibles para filtro
    public static List<string> ActionOptions { get; } = new()
    {
        "Create", "Update", "Delete", "Login", "Logout", "CancelSale",
        "BackupCreated", "BackupRestored", "ExportReport", "LowStockAlert"
    };

    // Lista de entidades posibles para filtro
    public static List<string> EntityOptions { get; } = new()
    {
        "Product", "Category", "Customer", "Supplier", "Sale",
        "User", "Company", "InventoryMovement"
    };

    public AuditLogViewModel(
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _auditService = auditService;
        _currentUserService = currentUserService;
        ViewTitle = "Auditoría";
    }

    public override async Task LoadAsync()
    {
        await LoadPageAsync(1);
    }

    private async Task LoadPageAsync(int page)
    {
        await ExecuteSafeAsync(async () =>
        {
            var filter = new PaginationFilter(page, PageSize);
            var companyId = _currentUserService.CompanyId;
            var result = await _auditService.GetAuditLogsAsync(
                companyId, filter, ActionFilter, EntityFilter, StartDate, EndDate);

            if (result.IsSuccess && result.Data is not null)
            {
                Items = new ObservableCollection<AuditLogDto>(result.Data.Items);
                TotalCount = result.Data.TotalCount;
                CurrentPage = result.Data.PageNumber;
                TotalPages = result.Data.TotalPages;
                PaginationText = $"Página {CurrentPage} de {TotalPages} ({TotalCount} registros)";
                HasPreviousPage = CurrentPage > 1;
                HasNextPage = CurrentPage < TotalPages;
            }
        }, "Cargar auditoría");
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadPageAsync(1);
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        ActionFilter = null;
        EntityFilter = null;
        StartDate = DateTime.Today.AddDays(-30);
        EndDate = DateTime.Today;
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
    private void ViewDetail(object? param)
    {
        if (param is not AuditLogDto log) return;
        SelectedLog = log;
        IsViewingDetail = true;
    }

    [RelayCommand]
    private void CloseDetail()
    {
        IsViewingDetail = false;
        SelectedLog = null;
    }
}
