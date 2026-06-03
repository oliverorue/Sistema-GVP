using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.WPF.ViewModels;

public partial class CategoriesViewModel : BaseViewModel
{
    private readonly ICategoryService _categoryService;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private ObservableCollection<CategoryDto> _items = new();

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private CategoryDto? _editingCategory;

    [ObservableProperty]
    private bool _isNew;

    public CategoriesViewModel(
        ICategoryService categoryService,
        ICurrentUserService currentUserService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _categoryService = categoryService;
        _currentUserService = currentUserService;
        ViewTitle = "Categorías";
    }

    public override async Task LoadAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var companyId = _currentUserService.CompanyId;
            var result = await _categoryService.GetAllAsync(companyId);
            if (result.IsSuccess && result.Data is not null)
            {
                Items = new ObservableCollection<CategoryDto>(result.Data);
            }
        }, "Cargar categorías");
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var companyId = _currentUserService.CompanyId;
            var result = await _categoryService.GetAllAsync(companyId);
            if (result.IsSuccess && result.Data is not null)
            {
                var filtered = string.IsNullOrWhiteSpace(SearchTerm)
                    ? result.Data
                    : result.Data.Where(c =>
                        c.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (c.Description?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                Items = new ObservableCollection<CategoryDto>(filtered);
            }
        }, "Buscar categorías");
    }

    [RelayCommand]
    private void NewCategory()
    {
        EditingCategory = new CategoryDto
        {
            CompanyId = _currentUserService.CompanyId,
            IsActive = true
        };
        IsNew = true;
        IsEditing = true;
        ClearError();
    }

    [RelayCommand]
    private void EditCategory(object? param)
    {
        if (param is not CategoryDto category) return;
        EditingCategory = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CompanyId = category.CompanyId,
            IsActive = category.IsActive
        };
        IsNew = false;
        IsEditing = true;
        ClearError();
    }

    [RelayCommand]
    private async Task SaveCategoryAsync()
    {
        if (EditingCategory is null) return;
        await ExecuteSafeAsync(async () =>
        {
            if (IsNew)
            {
                var result = await _categoryService.CreateAsync(EditingCategory);
                if (result.IsSuccess)
                {
                    await SetTemporaryStatusAsync("Categoría creada exitosamente");
                    IsEditing = false;
                    await LoadAsync();
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            else
            {
                var result = await _categoryService.UpdateAsync(EditingCategory);
                if (result.IsSuccess)
                {
                    await SetTemporaryStatusAsync("Categoría actualizada exitosamente");
                    IsEditing = false;
                    await LoadAsync();
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
        }, IsNew ? "Crear categoría" : "Actualizar categoría");
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(object? param)
    {
        if (param is not CategoryDto category) return;
        // El diálogo de confirmación lo manejaría IDialogService, aquí delegamos
        await ExecuteSafeAsync(async () =>
        {
            var result = await _categoryService.DeleteAsync(category.Id);
            if (result.IsSuccess)
            {
                await SetTemporaryStatusAsync("Categoría eliminada exitosamente");
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }, "Eliminar categoría");
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        EditingCategory = null;
        ClearError();
    }
}
