using System.Windows;
using OC.Assistant.Sdk;

namespace OC.Assistant.Plugins;

/// <summary>
/// UI to show and edit a <see cref="Plugin"/>.
/// </summary>
internal partial class PluginEditor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginEditor"/> class.
    /// </summary>
    public PluginEditor()
    {
        Visibility = Visibility.Collapsed;
        InitializeComponent();
    }

    /// <summary>
    /// Enable/Disable the control.
    /// </summary>
    public new bool IsEnabled
    {
        set => ApplyButton.IsEnabled = EditorWindow.IsEnabled = value;
    }
    
    public event Action<Plugin>? Saved;
    
    /// <summary>
    /// The editor has been closed.
    /// </summary>
    public event Action? Closed;
    
    /// <summary>
    /// Show the editor.
    /// </summary>
    /// <param name="plugin">The selected plugin to show.</param>
    /// <param name="plugins">All currently available plugins in the project.</param>
    /// <returns></returns>
    public async Task<bool> Show(Plugin plugin, IReadOnlyCollection<Plugin> plugins)
    {
        if (!await CheckAndConfirmChanges()) return false;
        EditorWindow.Show(plugins, plugin);
        Visibility = Visibility.Visible;
        
        plugin.IsSelected = true;
        foreach (var item in plugins.Where(p => p != plugin))
        {
            item.IsSelected = false;
        }
        
        return true;
    }

    public void Hide()
    {
        EditorWindow.ResetUnsavedChanges();
        foreach (var plugin in EditorWindow.Plugins)
        {
            plugin.IsSelected = false;
        }
        Visibility = Visibility.Collapsed;
    }

    private async Task<bool> CheckAndConfirmChanges()
    {
        if (!EditorWindow.UnsavedChanges) return true;

        var result = await Theme.MessageBox.Show(
            "Plugins", 
            """
            Changes have not been saved.
            Save now?
            """,
            MessageBoxButton.YesNoCancel, 
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return result == MessageBoxResult.No;
        }
        
        EditorWindow.Apply();
        return true;
    }

    private void EditorWindowOnChanged(bool isUnsavedChanges) =>
        ApplyButton.Visibility = isUnsavedChanges ? Visibility.Visible : Visibility.Collapsed;
    
    private void EditorWindowOnSaved(Plugin plugin) => 
        Saved?.Invoke(plugin);
    
    private void ApplyButtonClick(object sender, RoutedEventArgs e) => 
        EditorWindow.Apply();
    
    private void DiscardOnClick(object sender, RoutedEventArgs e) => 
        EditorWindow.Reload();
    
    private async void CloseButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!await CheckAndConfirmChanges()) return;
            Hide();
            Closed?.Invoke();
        }
        catch (Exception ex)
        {
            Logger.LogError(this, ex.Message);
        }
    }
}