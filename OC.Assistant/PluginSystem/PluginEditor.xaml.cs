using System.Windows;

namespace OC.Assistant.PluginSystem;

/// <summary>
/// UI to show and edit a <see cref="PluginSystem.Plugin"/>.
/// </summary>
internal partial class PluginEditor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginEditor"/> class.
    /// </summary>
    public PluginEditor()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The plugin has been added or saved.
    /// </summary>
    public event Action<Plugin, string?, Type?>? Saved;
    
    /// <summary>
    /// Selects and shows the given plugin.
    /// </summary>
    /// <param name="plugin">The selected plugin to show.</param>
    /// <param name="plugins">All currently available plugins in the project.</param>
    /// <returns></returns>
    public async Task Select(Plugin plugin, IReadOnlyCollection<Plugin> plugins)
    {
        if (!await CheckUnsavedChanges()) return;
        EditorWindow.Show(plugins, plugin);
        EditorWindow.Visibility = Visibility.Visible;
        
        plugin.IsSelected = true;
        foreach (var item in plugins.Where(p => p != plugin))
        {
            item.IsSelected = false;
        }
    }

    /// <summary>
    /// Clears the editor.
    /// </summary>
    public void Clear()
    {
        EditorWindow.ResetUnsavedChanges();
        foreach (var plugin in EditorWindow.Plugins)
        {
            plugin.IsSelected = false;
        }
        EditorWindow.Visibility = Visibility.Collapsed;
    }

    public async Task<bool> CheckUnsavedChanges()
    {
        if (!EditorWindow.UnsavedChanges) return true;

        await Theme.MessageBox.Show(
            "Unsaved changes", 
            """
            Unsaved changes for the selected plugin.
            Please confirm or discard.
            """,
            MessageBoxButton.OK, 
            MessageBoxImage.Warning);
        
        return false;
    }

    private void EditorWindowOnChanged(bool isUnsavedChanges) =>
        ApplyButton.Visibility = isUnsavedChanges ? Visibility.Visible : Visibility.Collapsed;
    
    private void EditorWindowOnSaved(Plugin plugin, string? oldName, Type? oldChannel) => 
        Saved?.Invoke(plugin, oldName, oldChannel);
    
    private async void ApplyButtonOnClick(object sender, RoutedEventArgs e)
    {
        Sdk.BusyState.Set(this);
        await EditorWindow.ApplyAsync();
        Sdk.BusyState.Reset(this);
    }
    
    private void DiscardOnClick(object sender, RoutedEventArgs e) => 
        EditorWindow.Reload();
}