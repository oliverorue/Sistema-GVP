using System;
using System.Linq;
using System.Windows;

namespace SistemaGVP.WPF.Services;

/// <summary>
/// Servicio para gestionar el cambio de tema claro/oscuro en la aplicación WPF.
/// Usa ResourceDictionary swap para cambiar entre LightTheme y DarkTheme en runtime.
/// Persiste la preferencia del usuario y notifica cambios.
/// </summary>
public class ThemeService
{
    private const string ThemePreferenceKey = "AppTheme";
    private bool _isDarkTheme;

    /// <summary>
    /// Evento disparado cuando el tema cambia. Parámetro bool indica si es dark.
    /// </summary>
    public event EventHandler<bool>? ThemeChanged;

    /// <summary>
    /// Indica si el tema actual es oscuro.
    /// </summary>
    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        private set
        {
            if (_isDarkTheme != value)
            {
                _isDarkTheme = value;
                ThemeChanged?.Invoke(this, value);
            }
        }
    }

    public ThemeService()
    {
        // Cargar tema guardado o usar claro por defecto
        _isDarkTheme = LoadThemePreference();
    }

    /// <summary>
    /// Alterna entre tema claro y oscuro
    /// </summary>
    public void ToggleTheme()
    {
        SetTheme(!IsDarkTheme);
    }

    /// <summary>
    /// Establece un tema específico (true = oscuro, false = claro)
    /// </summary>
    public void SetTheme(bool isDark)
    {
        IsDarkTheme = isDark;

        // Aplicar el tema a la aplicación mediante ResourceDictionary swap
        // NOTA: NO hacer MergedDictionaries.Clear() porque borraría BaseStyles.xaml.
        // Buscamos y reemplazamos solo el diccionario de tema (LightTheme o DarkTheme).
        if (System.Windows.Application.Current != null)
        {
            var merged = System.Windows.Application.Current.Resources.MergedDictionaries;
            var themeUri = new Uri(
                isDark ? "Styles/DarkTheme.xaml" : "Styles/LightTheme.xaml",
                UriKind.Relative);

            // Buscar si ya existe un diccionario de tema y reemplazarlo
            var existingTheme = merged.FirstOrDefault(d =>
            {
                var src = d.Source?.ToString();
                return src != null &&
                       (src.EndsWith("LightTheme.xaml") || src.EndsWith("DarkTheme.xaml"));
            });

            if (existingTheme != null)
            {
                var idx = merged.IndexOf(existingTheme);
                merged[idx] = new ResourceDictionary { Source = themeUri };
            }
            else
            {
                // Primera vez: insertar al inicio para que BaseStyles pueda sobrescribir si es necesario
                merged.Insert(0, new ResourceDictionary { Source = themeUri });
            }
        }

        // Guardar preferencia
        SaveThemePreference(isDark);
    }

    /// <summary>
    /// Obtiene el icono apropiado para el toggle (sol/luna)
    /// </summary>
    public string GetThemeIcon() => IsDarkTheme ? "☀️" : "🌙";

    /// <summary>
    /// Obtiene el tooltip para el botón de toggle
    /// </summary>
    public string GetThemeTooltip() => IsDarkTheme
        ? "Cambiar a tema claro"
        : "Cambiar a tema oscuro";

    private bool LoadThemePreference()
    {
        try
        {
            var savedTheme = Environment.GetEnvironmentVariable(ThemePreferenceKey);
            return savedTheme == "Dark";
        }
        catch
        {
            return false;
        }
    }

    private void SaveThemePreference(bool isDark)
    {
        try
        {
            var themeValue = isDark ? "Dark" : "Light";
            Environment.SetEnvironmentVariable(ThemePreferenceKey, themeValue, EnvironmentVariableTarget.User);
        }
        catch
        {
            // Ignorar errores al guardar preferencia
        }
    }
}
