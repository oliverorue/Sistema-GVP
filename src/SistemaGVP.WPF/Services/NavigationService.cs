using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace SistemaGVP.WPF.Services;

/// <summary>
/// Servicio de navegación que gestiona el cambio de vistas en un ContentControl.
/// Implementación adaptada para WPF.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<Type> _history = new();
    private ContentControl? _contentControl;
    private IServiceScope? _currentScope;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool CanGoBack => _history.Count > 1;

    public void Initialize(ContentControl contentControl)
    {
        _contentControl = contentControl;
    }

    public async Task NavigateTo<T>() where T : class
    {
        await NavigateTo(typeof(T));
    }

    public Task NavigateTo(Type viewType)
    {
        if (_contentControl == null)
            throw new InvalidOperationException("NavigationService no inicializado. Llame a Initialize primero.");

        // Dispose el ViewModel de la vista saliente para liberar suscripciones
        if (_contentControl.Content is UserControl oldView && oldView.DataContext is IDisposable disposableVm)
        {
            disposableVm.Dispose();
        }

        // Dispose previous scope before creating a new one to avoid memory leaks
        _currentScope?.Dispose();

        // Create a new DI scope so that scoped services can be resolved
        _currentScope = _serviceProvider.CreateScope();

        if (_contentControl.Content != null)
            _history.Push(_contentControl.Content.GetType());

        var resolved = _currentScope.ServiceProvider.GetRequiredService(viewType);

        var view = resolved as UserControl;
        if (view == null)
        {
            throw new InvalidOperationException($"El tipo {viewType.Name} no es un UserControl válido. Tipo real: {resolved?.GetType().FullName}");
        }

        _contentControl.Content = view;

        return Task.CompletedTask;
    }

    public Task GoBack()
    {
        if (!CanGoBack || _contentControl == null)
            return Task.CompletedTask;

        // Dispose el ViewModel de la vista saliente
        if (_contentControl.Content is UserControl oldView && oldView.DataContext is IDisposable disposableVm)
        {
            disposableVm.Dispose();
        }

        // Dispose previous scope and create a new one for the back-navigation view
        _currentScope?.Dispose();
        _currentScope = _serviceProvider.CreateScope();

        var previousViewType = _history.Pop();
        var view = _currentScope.ServiceProvider.GetRequiredService(previousViewType) as UserControl;
        _contentControl.Content = view;

        return Task.CompletedTask;
    }
}
