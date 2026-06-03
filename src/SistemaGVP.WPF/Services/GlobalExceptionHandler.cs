using System.Windows.Threading;
using Serilog;

namespace SistemaGVP.WPF.Services;

public sealed class GlobalExceptionHandler : IDisposable
{
    private readonly ILogger _logger;
    private bool _attached;

    public GlobalExceptionHandler(ILogger logger)
    {
        _logger = logger;
    }

    public void Attach()
    {
        if (_attached) return;
        _attached = true;

        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        System.Windows.Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;
    }

    public void Detach()
    {
        if (!_attached) return;
        _attached = false;

        AppDomain.CurrentDomain.UnhandledException -= OnAppDomainUnhandledException;
        System.Windows.Application.Current.DispatcherUnhandledException -= OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnTaskSchedulerUnobservedTaskException;
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        _logger.Fatal(ex, "Excepción no controlada del AppDomain. Terminating: {IsTerminating}", e.IsTerminating);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logger.Error(e.Exception, "Excepción no controlada del Dispatcher WPF.");
        e.Handled = true;
    }

    private void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger.Error(e.Exception, "Excepción no observada en Task.");
        e.SetObserved();
    }

    public void Dispose()
    {
        Detach();
    }
}
