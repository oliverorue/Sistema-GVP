using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SistemaGVP.WPF.Services;

/// <summary>
/// Implementación de IDialogService para WPF.
/// Crea ventanas de diálogo modales programáticamente.
/// </summary>
public class DialogService : IDialogService
{
    public Task ShowInfoAsync(string message, string title = "Información")
    {
        return ShowMessageBoxAsync(message, title, DialogType.Info);
    }

    public Task ShowWarningAsync(string message, string title = "Advertencia")
    {
        return ShowMessageBoxAsync(message, title, DialogType.Warning);
    }

    public Task ShowErrorAsync(string message, string title = "Error")
    {
        return ShowMessageBoxAsync(message, title, DialogType.Error);
    }

    public Task<bool> ShowConfirmAsync(string message, string title = "Confirmar")
    {
        return ShowConfirmDialogAsync(message, title);
    }

    public Task<string?> ShowInputAsync(string message, string title = "Entrada", string defaultValue = "")
    {
        return ShowInputDialogAsync(message, title, defaultValue);
    }

    private async Task ShowMessageBoxAsync(string message, string title, DialogType type)
    {
        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var owner = GetMainWindow();
            if (owner == null) return;

            var iconColor = type switch
            {
                DialogType.Info => new SolidColorBrush(Color.FromArgb(255, 25, 118, 210)),
                DialogType.Warning => new SolidColorBrush(Color.FromArgb(255, 245, 127, 23)),
                DialogType.Error => new SolidColorBrush(Color.FromArgb(255, 211, 47, 47)),
                _ => new SolidColorBrush(Color.FromArgb(255, 25, 118, 210))
            };

            var iconText = type switch
            {
                DialogType.Info => "ℹ️",
                DialogType.Warning => "⚠️",
                DialogType.Error => "❌",
                _ => "ℹ️"
            };

            var dialog = new Window
            {
                Title = title,
                Width = 420,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                Owner = owner,
                Content = new DockPanel
                {
                    Margin = new Thickness(20),
                    LastChildFill = true,
                    Children =
                    {
                        new StackPanel
                        {
                            VerticalAlignment = VerticalAlignment.Top,
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 0, 0, 15),
                            Children =
                            {
                                new Label
                                {
                                    Content = iconText,
                                    FontSize = 32,
                                    Margin = new Thickness(0, 0, 15, 0),
                                    VerticalAlignment = VerticalAlignment.Center
                                },
                                new Label
                                {
                                    Content = title,
                                    FontSize = 18,
                                    FontWeight = FontWeights.Bold,
                                    Foreground = iconColor,
                                    VerticalAlignment = VerticalAlignment.Center
                                }
                            }
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 14,
                            Margin = new Thickness(0, 5, 0, 15),
                            VerticalAlignment = VerticalAlignment.Stretch
                        },
                        new Button
                        {
                            Content = "Aceptar",
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Width = 100,
                            Height = 35,
                            Margin = new Thickness(0, 0, 0, 0)
                        }
                    }
                }
            };

            // Wire up the accept button to close
            var dockPanel = (DockPanel)dialog.Content;
            var acceptButton = dockPanel.Children.OfType<Button>().FirstOrDefault() ?? throw new System.InvalidOperationException("No se encontr� el bot�n de aceptar en el di�logo.");
            acceptButton.Click += (_, _) => dialog.Close();

            dialog.ShowDialog();
        });
    }

    private async Task<bool> ShowConfirmDialogAsync(string message, string title)
    {
        var tcs = new TaskCompletionSource<bool>();

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var owner = GetMainWindow();
            if (owner == null)
            {
                tcs.TrySetResult(false);
                return;
            }

            var dialog = new Window
            {
                Title = title,
                Width = 420,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                Owner = owner,
                Content = new DockPanel
                {
                    Margin = new Thickness(20),
                    LastChildFill = true,
                    Children =
                    {
                        new StackPanel
                        {
                            VerticalAlignment = VerticalAlignment.Top,
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 0, 0, 15),
                            Children =
                            {
                                new Label
                                {
                                    Content = "❓",
                                    FontSize = 32,
                                    Margin = new Thickness(0, 0, 15, 0),
                                    VerticalAlignment = VerticalAlignment.Center
                                },
                                new Label
                                {
                                    Content = title,
                                    FontSize = 18,
                                    FontWeight = FontWeights.Bold,
                                    Foreground = new SolidColorBrush(Color.FromArgb(255, 25, 118, 210)),
                                    VerticalAlignment = VerticalAlignment.Center
                                }
                            }
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 14,
                            Margin = new Thickness(0, 5, 0, 15),
                            VerticalAlignment = VerticalAlignment.Stretch
                        },
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Children =
                            {
                                new Button
                                {
                                    Content = "Cancelar",
                                    Width = 100,
                                    Height = 35,
                                    Margin = new Thickness(0, 0, 10, 0)
                                },
                                new Button
                                {
                                    Content = "Confirmar",
                                    Width = 100,
                                    Height = 35
                                }
                            }
                        }
                    }
                }
            };

            // Wire up buttons
            var panel = (DockPanel)dialog.Content;
            var buttonPanel = (StackPanel)panel.Children[2];
            var cancelButton = (Button)buttonPanel.Children[0];
            var confirmButton = (Button)buttonPanel.Children[1];

            cancelButton.Click += (_, _) =>
            {
                tcs.TrySetResult(false);
                dialog.Close();
            };
            confirmButton.Click += (_, _) =>
            {
                tcs.TrySetResult(true);
                dialog.Close();
            };

            dialog.ShowDialog();
            tcs.TrySetResult(false);
        });

        return await tcs.Task;
    }

    private async Task<string?> ShowInputDialogAsync(string message, string title, string defaultValue)
    {
        var tcs = new TaskCompletionSource<string?>();

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var owner = GetMainWindow();
            if (owner == null)
            {
                tcs.TrySetResult(defaultValue);
                return;
            }

            var inputBox = new TextBox
            {
                Text = defaultValue,
                Margin = new Thickness(0, 10, 0, 15)
            };

            var dialog = new Window
            {
                Title = title,
                Width = 420,
                Height = 230,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                Owner = owner,
                Content = new DockPanel
                {
                    Margin = new Thickness(20),
                    LastChildFill = true,
                    Children =
                    {
                        new Label
                        {
                            Content = title,
                            FontSize = 18,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 25, 118, 210)),
                            Margin = new Thickness(0, 0, 0, 5)
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 14,
                            Margin = new Thickness(0, 5, 0, 5)
                        },
                        inputBox,
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Children =
                            {
                                new Button
                                {
                                    Content = "Cancelar",
                                    Width = 100,
                                    Height = 35,
                                    Margin = new Thickness(0, 0, 10, 0)
                                },
                                new Button
                                {
                                    Content = "Aceptar",
                                    Width = 100,
                                    Height = 35
                                }
                            }
                        }
                    }
                }
            };

            // Wire up buttons
            var panel = (DockPanel)dialog.Content;
            var buttonPanel = (StackPanel)panel.Children[3];
            var cancelButton = (Button)buttonPanel.Children[0];
            var acceptButton = (Button)buttonPanel.Children[1];

            cancelButton.Click += (_, _) =>
            {
                tcs.TrySetResult(null);
                dialog.Close();
            };
            acceptButton.Click += (_, _) =>
            {
                tcs.TrySetResult(inputBox.Text);
                dialog.Close();
            };

            // Handle Enter key
            inputBox.KeyDown += (_, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    tcs.TrySetResult(inputBox.Text);
                    dialog.Close();
                }
            };

            // Focus the input box
            inputBox.Focus();

            dialog.ShowDialog();
            tcs.TrySetResult(null);
        });

        return await tcs.Task;
    }

    private static Window? GetMainWindow()
    {
        return System.Windows.Application.Current?.MainWindow;
    }

    private enum DialogType
    {
        Info,
        Warning,
        Error
    }
}
