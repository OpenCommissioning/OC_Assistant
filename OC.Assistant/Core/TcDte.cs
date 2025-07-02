using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Core;

/// <summary>
/// Represents a static class with methods to extend the <see cref="EnvDTE.DTE"/> interface for TwinCAT specific usings.
/// </summary>
public static class TcDte
{
    /// <summary>
    /// Supported versions, prioritized in this order:<br/><br/>
    /// <c>TcXaeShell.DTE.17.0</c> : TwinCAT Shell based on VS2022<br/>
    /// <c>TcXaeShell.DTE.15.0</c> : TwinCAT Shell based on VS2017<br/>
    /// <c>VisualStudio.DTE.17.0</c> : Visual Studio 2022<br/>
    /// <c>VisualStudio.DTE.16.0</c> : Visual Studio 2019<br/>
    /// <c>VisualStudio.DTE.15.0</c> : Visual Studio 2017<br/>
    /// </summary>
    private static readonly Type? InstalledShell = 
        Type.GetTypeFromProgID("TcXaeShell.DTE.17.0") ??
        Type.GetTypeFromProgID("TcXaeShell.DTE.15.0") ??
        Type.GetTypeFromProgID("VisualStudio.DTE.17.0") ??
        Type.GetTypeFromProgID("VisualStudio.DTE.16.0") ??
        Type.GetTypeFromProgID("VisualStudio.DTE.15.0");
    
    /// <summary>
    /// Creates a new Visual Studio instance.
    /// </summary>
    /// <returns>The <see cref="DTE"/> interface to the created instance.</returns>
    /// <exception cref="Exception">An appropriate shell is not installed on the computer.</exception>
    /// <exception cref="Exception">Creating an instance of the shell failed.</exception>
    public static DTE Create()
    {
        if (InstalledShell is null)
        {
            throw new Exception("No TwinCAT Shell installed");
        }
        
        Logger.LogInfo(typeof(TcDte), "Create TwinCAT Shell instance ...");

        if (Activator.CreateInstance(InstalledShell) is not DTE dte)
        {
            throw new Exception("Creating instance of TwinCAT Shell failed");
        }
        
        return dte;
    }
    
    /// <summary>
    /// A collection to store COM objects used across TwinCAT operations.
    /// This set ensures COM objects are tracked and can be properly finalized
    /// or released when no longer needed to prevent resource leaks.
    /// </summary>
    public static ConcurrentBag<object?> ComObjects2 { get; } = [];
    
    /// <summary>
    /// Releases all references of the given COM object.
    /// </summary>
    /// <param name="comObject">The given COM object.</param>
    public static void Finalize<T>(this T? comObject) where T : class
    {
        if (comObject is null || !Marshal.IsComObject(comObject)) return;
        Marshal.FinalReleaseComObject(comObject);
    }
    
    /// <summary>
    /// Tries to get the path of the project folder.
    /// </summary>
    /// <param name="dte">The given <see cref="DTE"/> interface.</param>
    /// <returns>The path of the project folder if succeeded, otherwise <see langword="null"/>.</returns>
    public static string? GetProjectFolder(this DTE? dte)
    {
        var project = GetTcProject(dte);
        var path = project is null ? null : Directory.GetParent(project.FullName)?.FullName;
        project.Finalize();
        return path;
    }

    /// <summary>
    /// Tries to get the <see cref="ITcSysManager15"/>.
    /// </summary>
    /// <param name="dte">The given <see cref="DTE"/> interface.</param>
    /// <returns>The <see cref="ITcSysManager15"/> interface if succeeded, otherwise <see langword="null"/>.</returns>
    public static ITcSysManager15? GetTcSysManager(this DTE? dte)
    {
        var project = GetTcProject(dte);
        if (project is null) return null;
        var tcSysManager = project.Object as ITcSysManager15;
        project.Finalize();
        return tcSysManager;
    }
    

    /// <summary>
    /// Tries to get the first <see cref="Project"/> of the given <see cref="DTE"/> that implements <see cref="ITcSysManager15"/>.
    /// </summary>
    /// <param name="dte">The given <see cref="DTE"/> interface.</param>
    /// <returns>The first <see cref="Project"/> implementing <see cref="ITcSysManager15"/> if succeeded, otherwise <see langword="null"/>.</returns>
    public static Project? GetTcProject(this DTE? dte)
    {
        if (dte is null) return null;
        foreach (Project project in dte.Solution.Projects)
        {
            if (project.Object is ITcSysManager15)
            {
                return project;
            }
            project.Finalize();
        }
        return null;
    }
    
    /// <summary>
    /// Gets the <see cref="DTE"/> interface of the given solution path.
    /// </summary>
    /// <param name="solutionFullName">The full name of the solution file.</param>
    /// <returns>The <see cref="DTE"/> interface of the given solution path if any, otherwise null.</returns>
    public static DTE? GetInstance(string solutionFullName)
    {
        return GetInstances(solutionFullName).FirstOrDefault();
    }
    
    /// <summary>
    /// Returns a collection of <see cref="DTE"/> by querying all supported Visual Studio instances
    /// with a valid TwinCAT solution.
    /// </summary>
    /// <returns>A collection of <see cref="DTE"/> interfaces with a valid TwinCAT solution.</returns> 
    public static IEnumerable<DTE> GetInstances(string? solutionFullName = null)
    {
        if (InstalledShell is null) yield break;
        if (GetRunningObjectTable(0, out var runningObjectTable) != 0) yield break;
        runningObjectTable.EnumRunning(out var enumMoniker);

        var fetched = IntPtr.Zero;
        var moniker = new IMoniker[1];

        while (enumMoniker.Next(1, moniker, fetched) == 0)
        {
            DTE? dte = null;
            
            try
            {
                CreateBindCtx(0, out var bindCtx);
                moniker[0].GetDisplayName(bindCtx, null, out var displayName);
                
                if (!displayName.StartsWith("!TcXaeShell.DTE") && !displayName.StartsWith("!VisualStudio.DTE")) continue;
                if (runningObjectTable.GetObject(moniker[0], out var obj) != 0) continue;
                
                dte = (DTE?) obj;
                var fullName = dte?.Solution?.FullName;
                
                if (string.IsNullOrEmpty(fullName))
                {
                    dte?.Finalize();
                    continue;
                }

                if (!string.IsNullOrEmpty(solutionFullName) &&
                    !string.Equals(solutionFullName, fullName, StringComparison.OrdinalIgnoreCase))
                {
                    dte?.Finalize();
                    continue;
                }
                
                if (dte?.GetTcProject() is not {} tcProject)
                {
                    dte?.Finalize();
                    continue;
                }
                
                tcProject.Finalize();
            }
            catch (Exception e)
            {
                dte?.Finalize();
                Logger.LogError(typeof(TcDte), e.Message, true);
                continue;
            }
            
            yield return dte;
        }
    }

    [DllImport("ole32.dll")]
    private static extern void CreateBindCtx(int reserved, out IBindCtx bindCtx);

    [DllImport("ole32.dll")]
    private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable runningObjectTable);
}