using System.Windows;
using System.Windows.Controls;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Plugins;

internal partial class EditorWindow
{
    private Plugin Plugin { get; set; } = new ();
    public IReadOnlyCollection<Plugin> Plugins { get; set; } = [];
    
    public EditorWindow()
    {
        InitializeComponent();
    }
    
    public void Show(IReadOnlyCollection<Plugin> plugins, Plugin plugin)
    {
        TypeDropdown.Visibility = Visibility.Collapsed;

        Plugin = plugin;
        Plugins = plugins;
        
        PluginName.TextChanged -= ValueOnChanged;
        PluginName.Text = Plugin.Name;
        PluginName.TextChanged += ValueOnChanged;
        TypeInfo.Text = Plugin.Type?.Name ?? "";
        
        LoadParameters();
    }

    public void Reload()
    {
        Show(Plugins, Plugin);
    }
    
    private void TypeSelectorOnSelected(Type e)
    {
        if (TypeDropdown.Visibility == Visibility.Collapsed || e == Plugin.Type) return;
        if (Plugin.InitType(e) != true) return;
        LoadParameters();
    }
        
    private void LoadParameters()
    {
        ResetUnsavedChanges();

        if (!Plugin.IsValid && !Plugin.InitType(TypeDropdown.SelectedType)) return;
        
        ParameterPanel.Children.Clear();
        foreach (var parameter in Plugin.PluginController.Parameter
                     .ToList().Select(p => new PluginParameter(p)))
        {
            parameter.Changed += () => ValueOnChanged();
            ParameterPanel.Children.Add(parameter);
        }
    }
    
    private void ValueOnChanged(object? sender = null, TextChangedEventArgs? e = null)
    {
        UnsavedChanges = true;
        Changed?.Invoke(true);
    }

    public void ResetUnsavedChanges()
    {
        UnsavedChanges = false;
        Changed?.Invoke(false);
    }
    
    public bool UnsavedChanges { get; private set; }
    public event Action<bool>? Changed;
    public event Action<Plugin, string?>? Saved;

    public bool Apply()
    {
        try
        {
            if (!PluginName.Text.IsPlcCompatible())
            {
                Logger.LogWarning(this, $"Name {PluginName.Text} is not PLC compatible");
                return false;
            }
        
            if (Plugins.Any(plugin => plugin.Name == PluginName.Text && plugin != Plugin))
            {
                Logger.LogWarning(this, $"Name {PluginName.Text} already exists");
                return false;
            }

            var oldName = Plugin.Name != PluginName.Text ? Plugin.Name : null;

            //Update parameters of the selected plugin
            Plugin.PluginController?.Parameter.Update(ParameterPanel.Children.OfType<IParameter>());
        
            //Call the save method for the selected plugin
            if (Plugin.Save(PluginName.Text) != true)
            {
                Logger.LogWarning(this, $"Unable to save plugin {Plugin.Name}");
                return false;
            }
        
            Logger.LogInfo(this, $"Plugin '{Plugin.Name}' saved");
            ResetUnsavedChanges();
            Saved?.Invoke(Plugin, oldName);
            return true;

        }
        catch (Exception ex)
        {
            Logger.LogError(this, ex.Message);
            return false;
        }
    }
}