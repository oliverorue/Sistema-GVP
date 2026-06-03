using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;

namespace SistemaGVP.WPF.ViewModels;

/// <summary>
/// ViewModel base con funcionalidades comunes.
/// Proporciona manejo de carga, errores y logging.
/// Hereda de ViewModelBase que ya implementa IDisposable.
/// </summary>
public abstract partial class BaseViewModel : ViewModelBase
{
    protected readonly ILogger<BaseViewModel>? _logger;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasStatusMessage;

    public string ViewTitle { get; protected set; } = string.Empty;

    protected BaseViewModel(ILogger<BaseViewModel>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta una acción de forma segura, manejando excepciones.
    /// </summary>
    protected async Task ExecuteSafeAsync(Func<Task> action, string errorContext = "")
    {
        if (IsBusy)
        {
            _logger?.LogWarning("ExecuteSafeAsync bloqueado por IsBusy=true en contexto: {Context}. " +
                "Esto indica que una operación previa no liberó el estado. Forzando liberación.", errorContext);
            IsBusy = false;
        }

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;
            StatusMessage = "Procesando...";

            await action();

            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error: {ex.Message}";
            _logger?.LogError(ex, "Error en {Context}: {Message}", errorContext, ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Establece un mensaje de estado temporal que se limpia después de un delay.
    /// </summary>
    protected async Task SetTemporaryStatusAsync(string message, int delayMs = 3000)
    {
        HasStatusMessage = true;
        StatusMessage = message;

        if (delayMs > 0)
        {
            await Task.Delay(delayMs);
            if (StatusMessage == message)
            {
                StatusMessage = string.Empty;
                HasStatusMessage = false;
            }
        }
    }

    /// <summary>
    /// Limpia el estado de error.
    /// </summary>
    protected void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Método de carga virtual para que las vistas llamen en el evento Loaded.
    /// </summary>
    public virtual async Task LoadAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Libera recursos administrados. Override en subclases para limpiar suscripciones.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Las subclases sobrescriben esto para limpiar suscripciones
        }
        base.Dispose(disposing);
    }
}
