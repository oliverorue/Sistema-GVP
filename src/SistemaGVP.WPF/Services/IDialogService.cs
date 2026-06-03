using System.Threading.Tasks;

namespace SistemaGVP.WPF.Services;

/// <summary>
/// Interfaz para mostrar cuadros de diálogo.
/// </summary>
public interface IDialogService
{
    Task ShowInfoAsync(string message, string title = "Información");
    Task ShowWarningAsync(string message, string title = "Advertencia");
    Task ShowErrorAsync(string message, string title = "Error");
    Task<bool> ShowConfirmAsync(string message, string title = "Confirmar");
    Task<string?> ShowInputAsync(string message, string title = "Entrada", string defaultValue = "");
}
