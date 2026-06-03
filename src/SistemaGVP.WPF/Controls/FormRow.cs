using System.Windows;
using System.Windows.Controls;

namespace SistemaGVP.WPF.Controls;

public class FormRow : ContentControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(FormRow), new PropertyMetadata(""));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    static FormRow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FormRow), new FrameworkPropertyMetadata(typeof(FormRow)));
    }
}
