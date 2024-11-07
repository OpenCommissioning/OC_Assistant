namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Internal interface to control the plugin.
/// </summary>
internal interface IPluginController
{
    /// <summary>
    /// Initializes the plugin with the given name.
    /// </summary>
    /// <param name="name">The name of the plugin.</param>
    void Initialize(string? name);
    
    /// <summary>
    /// Saves the plugin.
    /// </summary>
    /// <param name="name">The current name of the plugin.</param>
    /// <returns>True if successful, otherwise false.</returns>
    bool Save(string? name);
    
    /// <summary>
    /// Starts the plugin.
    /// </summary>
    void Start();
    
    /// <summary>
    /// Stops the plugin.
    /// </summary>
    void Stop();
    
    /// <summary>
    /// Interface for the parameter collection.
    /// </summary>
    IParameterCollection Parameter { get; }
    
    /// <summary>
    /// Is raised when the plugin has been started successfully.
    /// </summary>
    event Action Started;
    
    /// <summary>
    /// Is raised when the plugin has been stopped.
    /// </summary>
    event Action Stopped;
    
    /// <summary>
    /// Is raised when the plugin is about to start.
    /// </summary>
    event Action Starting;
    
    /// <summary>
    /// Is raised when the plugin is about to stop.
    /// </summary>
    event Action Stopping;
    
    /// <summary>
    /// <see cref="PluginIoType"/> value.
    /// </summary>
    IoType IoType { get; }
    
    /// <summary>
    /// <see cref="PluginDelayAfterStart"/> value.
    /// </summary>
    int DelayAfterStart { get; }
    
    /// <summary>
    /// Indicator that the plugin runs successfully after it has been started.
    /// </summary>
    bool IsRunning { get; }
    
    /// <summary>
    /// Autostart parameter.
    /// </summary>
    bool AutoStart { get; }
    
    /// <summary>
    /// Property to indicate whether to generate the plugin.
    /// </summary>
    bool IoChanged { get; }
    
    /// <summary>
    /// Is used when the attribute <see cref="PluginIoType"/> is set to <see cref="IoType.Struct"/>.<br/>
    /// Variables should be added in the <see cref="PluginBase.OnSave"/> method.
    /// </summary>
    IIoStructure InputStructure { get; }
    
    /// <summary>
    /// Is used when the attribute <see cref="PluginIoType"/> is set to <see cref="IoType.Struct"/>.<br/>
    /// Variables should be added in the <see cref="PluginBase.OnSave"/> method.
    /// </summary>
    IIoStructure OutputStructure { get; }
    
    /// <summary>
    /// Is used when the attribute <see cref="PluginIoType"/> is set to <see cref="IoType.Address"/>.<br/>
    /// Defines the plugin input addresses.
    /// </summary>
    int[] InputAddress { get; }
    
    /// <summary>
    /// Is used when the attribute <see cref="PluginIoType"/> is set to <see cref="IoType.Address"/>.<br/>
    /// Defines the plugin output addresses.
    /// </summary>
    int[] OutputAddress { get; }
}