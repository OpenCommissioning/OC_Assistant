namespace OC.Assistant.Plugins;

/// <summary>
/// Represents a plugin info, containing its type information and optional details.
/// </summary>
internal class PluginInfo(Type type, string? repositoryUrl = null, string? repositoryType = null)
{
    /// <summary>
    /// The <see cref="System.Type"/> of the plugin.
    /// </summary>
    public Type Type { get; } = type;
    /// <summary>
    /// The url of the plugin repository, if any.
    /// </summary>
    public string? RepositoryUrl { get; } = repositoryUrl;
    /// <summary>
    /// The type of the plugin repository, if any.
    /// </summary>
    public string? RepositoryType { get; } = repositoryType;
}