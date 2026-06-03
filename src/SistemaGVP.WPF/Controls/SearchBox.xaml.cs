using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SistemaGVP.WPF.Controls;

public partial class SearchBox : UserControl
{
    public static readonly DependencyProperty SearchTermProperty =
        DependencyProperty.Register(nameof(SearchTerm), typeof(string), typeof(SearchBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchBox),
            new PropertyMetadata("Buscar..."));

    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(SearchBox),
            new PropertyMetadata(null));

    public string SearchTerm
    {
        get => (string)GetValue(SearchTermProperty);
        set => SetValue(SearchTermProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public SearchBox()
    {
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter && SearchCommand?.CanExecute(SearchTerm) == true)
        {
            SearchCommand.Execute(SearchTerm);
        }
        base.OnKeyDown(e);
    }
}
