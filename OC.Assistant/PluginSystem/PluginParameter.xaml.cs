using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.PluginSystem;

internal partial class PluginParameter : IParameter
{
    public PluginParameter(IParameter parameter)
    {
        InitializeComponent();
        DockPanel.SetDock(this, Dock.Top);
        VerticalAlignment = VerticalAlignment.Top;
        Loaded += OnLoaded;
        
        Name = parameter.Name;
        Value = parameter.Value;
        ToolTip = parameter.ToolTip;
        FileFilter = parameter.FileFilter;
    }
        
    public new string Name
    {
        get => NameLabel.Content.ToString() ?? "";
        private init => NameLabel.Content = value;
    }
        
    public object? Value
    {
        get => ValueTextBox.Text.ConvertTo(field?.GetType() ?? null);
        set
        {
            Dispatcher.Invoke(() =>
            {
                field = value;
                ValueTextBox.Text = $"{field}";
                
                if (field is bool boolValue)
                {
                    ValueCheckBox.Visibility = Visibility.Visible;
                    ValueCheckBox.IsChecked = boolValue;
                    ValueTextBox.Visibility = Visibility.Hidden;
                    return;
                }
                
                ValueCheckBox.Visibility = Visibility.Hidden;
                ValueTextBox.Visibility = Visibility.Visible;
            });
        }
    }

    public string? FileFilter
    {
        get;
        private init
        {
            field = value;
            FileSelector.Visibility = field is null || Value is bool ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Margin = new Thickness(0,3,0,3);
    }

    private void FileSelector_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog {InitialDirectory = $"{Value}"};
        if (!string.IsNullOrEmpty(FileFilter))
        {
            openFileDialog.Filter = $"*.{FileFilter}|*.{FileFilter}"; 
        }
        if (openFileDialog.ShowDialog() is not true) return;
        Value = openFileDialog.FileName;
    }

    private void ValueCheckBox_OnChecked(object sender, RoutedEventArgs e)
    {
        Value = true;
    }

    private void ValueCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
    {
        Value = false;
    }

    private void ValueTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!ValueTextBox.IsKeyboardFocused) return;
        Changed?.Invoke();
    }

    private void ValueCheckBox_OnClick(object sender, RoutedEventArgs e)
    {
        Changed?.Invoke();
    }

    public event Action? Changed;
}