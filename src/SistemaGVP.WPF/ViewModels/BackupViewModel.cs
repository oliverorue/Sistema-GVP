using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.WPF.Services;

namespace SistemaGVP.WPF.ViewModels;

/// <summary>
/// ViewModel para la gestión de copias de seguridad (backups).
/// Permite listar, crear, restaurar y eliminar backups de la base de datos.
/// </summary>
public partial class BackupViewModel : BaseViewModel
{
    private readonly IBackupService _backupService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<BackupInfoDto> _items = new();

    [ObservableProperty]
    private bool _isOperationInProgress;

    public BackupViewModel(
        IBackupService backupService,
        ICurrentUserService currentUserService,
        IDialogService dialogService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _backupService = backupService;
        _currentUserService = currentUserService;
        _dialogService = dialogService;
        ViewTitle = "Copias de Seguridad";
    }

    public override async Task LoadAsync()
    {
        await LoadBackupsAsync();
    }

    /// <summary>
    /// Carga la lista de backups desde el servicio.
    /// </summary>
    private async Task LoadBackupsAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            _logger?.LogInformation("LoadBackupsAsync: Iniciando carga. CompanyId={CompanyId}", _currentUserService.CompanyId);

            var result = await _backupService.GetBackupsAsync(_currentUserService.CompanyId);

            if (result.IsSuccess)
            {
                Items = new ObservableCollection<BackupInfoDto>(result.Data ?? new List<BackupInfoDto>());
                _logger?.LogInformation("LoadBackupsAsync: {Count} backup(s) cargados.", Items.Count);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Message;
                _logger?.LogWarning("LoadBackupsAsync: Error al cargar backups: {Message}", result.Message);
            }
        }, "Cargar backups");
    }

    /// <summary>
    /// Crea un nuevo backup de la base de datos.
    /// </summary>
    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            IsOperationInProgress = true;

            _logger?.LogInformation("CreateBackupAsync: Creando backup. CompanyId={CompanyId}, UserId={UserId}",
                _currentUserService.CompanyId, _currentUserService.UserId);

            var result = await _backupService.CreateBackupAsync(
                _currentUserService.CompanyId,
                _currentUserService.UserId);

            if (result.IsSuccess)
            {
                await LoadBackupsAsync();
                await SetTemporaryStatusAsync("✅ Backup creado exitosamente", 3000);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Message;
            }
        }, "Crear backup");
    }

    /// <summary>
    /// Restaura un backup seleccionado. Solicita confirmación antes de proceder.
    /// </summary>
    [RelayCommand]
    private async Task RestoreBackupAsync(object? param)
    {
        if (param is not BackupInfoDto dto) return;

        var confirm = await _dialogService.ShowConfirmAsync(
            $"¿Restaurar el backup '{dto.FileName}'?\n\n" +
            "Esta acción reemplazará la base de datos actual. Se creará una copia de seguridad " +
            "automática del estado actual antes de restaurar.\n\n" +
            "IMPORTANTE: La aplicación deberá reiniciarse después de la restauración.",
            "Confirmar Restauración");

        if (!confirm) return;

        await ExecuteSafeAsync(async () =>
        {
            IsOperationInProgress = true;

            _logger?.LogInformation("RestoreBackupAsync: Restaurando backup. File={FilePath}, CompanyId={CompanyId}, UserId={UserId}",
                dto.FilePath, _currentUserService.CompanyId, _currentUserService.UserId);

            var result = await _backupService.RestoreBackupAsync(
                dto.FilePath,
                _currentUserService.CompanyId,
                _currentUserService.UserId);

            if (result.IsSuccess)
            {
                await LoadBackupsAsync();
                await SetTemporaryStatusAsync("✅ Backup restaurado. Reinicie la aplicación para aplicar los cambios.", 5000);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Message;
            }
        }, "Restaurar backup");
    }

    /// <summary>
    /// Elimina un archivo de backup del sistema de archivos.
    /// </summary>
    [RelayCommand]
    private async Task DeleteBackupAsync(object? param)
    {
        if (param is not BackupInfoDto dto) return;

        var confirm = await _dialogService.ShowConfirmAsync(
            $"¿Eliminar el backup '{dto.FileName}'?\n\n" +
            $"Tamaño: {dto.FileSizeDisplay}\n" +
            $"Fecha: {dto.CreatedAt:dd/MM/yyyy HH:mm}\n\n" +
            "Esta acción no se puede deshacer.",
            "Confirmar Eliminación");

        if (!confirm) return;

        await ExecuteSafeAsync(async () =>
        {
            IsOperationInProgress = true;

            _logger?.LogInformation("DeleteBackupAsync: Eliminando backup. File={FilePath}", dto.FilePath);

            try
            {
                // Eliminar el archivo .db
                if (File.Exists(dto.FilePath))
                {
                    File.Delete(dto.FilePath);
                }

                // Eliminar el archivo .metadata asociado si existe
                var metadataPath = Path.ChangeExtension(dto.FilePath, ".metadata");
                if (File.Exists(metadataPath))
                {
                    File.Delete(metadataPath);
                }

                await LoadBackupsAsync();
                await SetTemporaryStatusAsync("✅ Backup eliminado correctamente", 3000);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al eliminar archivo de backup: {File}", dto.FilePath);
                HasError = true;
                ErrorMessage = $"Error al eliminar el archivo: {ex.Message}";
            }
        }, "Eliminar backup");
    }

    /// <summary>
    /// Recarga la lista de backups manualmente.
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadBackupsAsync();
        await SetTemporaryStatusAsync("Lista de backups actualizada", 2000);
    }
}
