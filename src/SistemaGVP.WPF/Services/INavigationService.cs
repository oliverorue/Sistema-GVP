using System;
using System.Threading.Tasks;

namespace SistemaGVP.WPF.Services;

/// <summary>
/// Interfaz para el servicio de navegación entre vistas.
/// </summary>
public interface INavigationService
{
    Task NavigateTo<T>() where T : class;
    Task NavigateTo(Type viewType);
    Task GoBack();
    bool CanGoBack { get; }
}
