using System.Windows;
using System.Windows.Controls;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Plugins;

/// <summary>
/// UI to show and edit a <see cref="Plugin"/>.
/// </summary>
internal partial class PluginEditor
{
    private Plugin? _plugin;
    private IReadOnlyCollection<Plugin> _plugins = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginEditor"/> class.
    /// </summary>
    public PluginEditor()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Enable/Disable the control.
    /// </summary>
    public new bool IsEnabled
    {
        set => ApplyButton.IsEnabled = EditorWindow.IsEnabled = value;
    }

    /// <summary>
    /// The selected plugin has been saved.
    /// </summary>
    public event Action<Plugin>? OnConfirm;
    
    /// <summary>
    /// The editor has been closed.
    /// </summary>
    public event Action? OnCancel;
    
    /// <summary>
    /// Show the editor.
    /// </summary>
    /// <param name="plugin">The selected plugin to show.</param>
    /// <param name="plugins">All currently available plugins in the project.</param>
    /// <returns></returns>
    public bool Show(Plugin plugin, IReadOnlyCollection<Plugin> plugins)
    {
        var selection = (ComboBoxItem)TypeDropdown.SelectedItem;
        if (selection is null)
        {
            Logger.LogWarning(this, "No Plugins found");
            return false;
        }
        
        _plugins = plugins;
        _plugin = plugin;
        Visibility = Visibility.Visible;

        ShowParameter();
        PluginName.Text = _plugin.Name;
        selection.Content = _plugin?.Type?.Name ?? "";
        
        //Disable the name input and the type dropdown if the plugin has been saved already
        var isNew = plugins.All(x => x != plugin);
        PluginName.IsEnabled = isNew;
        TypeDropdown.IsEnabled = isNew;
        
        return true;
    }
    
    private void TypeSelectorOnSelected(Type e)
    {
        if (e == _plugin?.Type) return;
        if (_plugin?.InitType(e) != true) return;
        ShowParameter();
    }
        
    private void ShowParameter()
    {
        IndicateChanges = false;
        if (_plugin is null) return;
        
        if (!_plugin.IsValid)
        {
            if (!_plugin.InitType(TypeDropdown.SelectedType)) return;
        }

        ParameterPanel.Children.Clear();
        foreach (var parameter in _plugin.PluginController.Parameter.ToList())
        {
            var param = new PluginParameter(parameter);
            param.Changed += () => IndicateChanges = true;
            ParameterPanel.Children.Add(param);
        }
    }
    
    private bool IndicateChanges
    {
        set => ApplyButton.Content = value ? "Apply*" : "Apply";
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!PluginName.Text.IsPlcCompatible())
        {
            Theme.MessageBox.Show(PluginName.Text, "Name is not TwinCAT PLC compatible", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (_plugins.Any(plugin => plugin.Name == PluginName.Text && plugin != _plugin))
        {
            Theme.MessageBox.Show(PluginName.Text, "Name already exists", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (Theme.MessageBox.Show("Editor", $"Save {PluginName.Text}?", MessageBoxButton.OKCancel,
                MessageBoxImage.Question) == MessageBoxResult.Cancel)
        {
            return;
        }

        //Update parameters of selected plugin
        _plugin?.PluginController?.Parameter.Update(ParameterPanel.Children.OfType<IParameter>());
        
        //Call the save method for the selected plugin
        if (_plugin?.Save(PluginName.Text) != true) return;
        
        Logger.LogInfo(this, $"'{_plugin.Name}' saved");
        OnConfirm?.Invoke(_plugin);
        IndicateChanges = false;
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Hidden;
        OnCancel?.Invoke();
    }
}