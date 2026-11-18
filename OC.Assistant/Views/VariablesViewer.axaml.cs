using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace OC.Assistant.Views;

public partial class VariablesViewer : Window
{
    public VariablesViewer()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// Commits a forced value with Enter, discards the edit with Escape.
    /// </summary>
    private static void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Source is not TextBox { IsReadOnly: false } textBox) return;
        var binding = BindingOperations.GetBindingExpressionBase(textBox, TextBox.TextProperty);

        switch (e.Key)
        {
            case Key.Enter:
                binding?.UpdateSource();
                e.Handled = true;
                return;
            case Key.Escape:
                binding?.UpdateTarget();
                e.Handled = true;
                return;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        (DataContext as IDisposable)?.Dispose();
    }
}
