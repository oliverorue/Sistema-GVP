using Microsoft.Extensions.Logging;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SistemaGVP.WPF.Services;

public class NotificationService : INotificationService
{
    private readonly ISnackbarService _snackbarService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ISnackbarService snackbarService, ILogger<NotificationService>? logger = null)
    {
        _snackbarService = snackbarService;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<NotificationService>.Instance;
    }

    public void ShowSuccess(string message, int timeoutMs = 5000)
    {
        _snackbarService.Show("Éxito", message, ControlAppearance.Success, null, TimeSpan.FromMilliseconds(timeoutMs));
    }

    public void ShowError(string message, int timeoutMs = 5000)
    {
        _logger.LogError("Notificación de error: {Message}", message);
        _snackbarService.Show("Error", message, ControlAppearance.Danger, null, TimeSpan.FromMilliseconds(timeoutMs));
    }

    public void ShowWarning(string message, int timeoutMs = 5000)
    {
        _snackbarService.Show("Advertencia", message, ControlAppearance.Caution, null, TimeSpan.FromMilliseconds(timeoutMs));
    }

    public void ShowInfo(string message, int timeoutMs = 5000)
    {
        _snackbarService.Show("Información", message, ControlAppearance.Info, null, TimeSpan.FromMilliseconds(timeoutMs));
    }
}
