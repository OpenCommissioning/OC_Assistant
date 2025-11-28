using System.Reflection;

namespace OC.Assistant.Common;

/// <summary>
/// Represents information for an <see cref="Assembly"/>.
/// </summary>
internal class AssemblyInfo(Assembly assembly, string? repositoryUrl = null, string? repositoryType = null)
{
    /// <summary>
    /// The <see cref="Assembly"/>.
    /// </summary>
    public Assembly Assembly { get; } = assembly;
    /// <summary>
    /// The url of the assembly's repository, if any.
    /// </summary>
    public string? RepositoryUrl { get; } = repositoryUrl;
    /// <summary>
    /// The type of the assembly's repository, if any.
    /// </summary>
    public string? RepositoryType { get; } = repositoryType;
}