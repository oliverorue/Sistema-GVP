namespace SistemaGVP.WPF.Services;

public interface INotificationService
{
    void ShowSuccess(string message, int timeoutMs = 5000);
    void ShowError(string message, int timeoutMs = 5000);
    void ShowWarning(string message, int timeoutMs = 5000);
    void ShowInfo(string message, int timeoutMs = 5000);
}
