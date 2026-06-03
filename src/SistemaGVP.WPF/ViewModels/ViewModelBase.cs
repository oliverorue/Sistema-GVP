using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace SistemaGVP.WPF.ViewModels;

/// <summary>
/// Clase base abstracta para todos los ViewModels.
/// Implementa IDisposable para permitir limpieza de suscripciones y recursos.
/// </summary>
public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Libera recursos. Las subclases deben hacer override y llamar a base.Dispose(disposing).
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Método virtual para liberar recursos administrados y no administrados.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            // Liberar recursos administrados en subclases
        }
        _disposed = true;
    }
}
