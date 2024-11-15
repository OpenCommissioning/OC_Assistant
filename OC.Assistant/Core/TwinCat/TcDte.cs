using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using OC.Assistant.Sdk;
using TCatSysManagerLib;
using EnvDTE;

namespace OC.Assistant.Core.TwinCat;

/// <summary>
/// Represents a wrapper around a <see cref="EnvDTE.DTE"/> for TwinCAT specific usings.
/// </summary>
public class TcDte
{
    private readonly DTE? _dte;
    private static readonly Type? InstalledShell = 
        Type.GetTypeFromProgID("TcXaeShell.DTE.17.0") ?? 
        Type.GetTypeFromProgID("TcXaeShell.DTE.15.0");
    
    /// <summary>
    /// Creates a new instance of the <see cref="TcDte"/> class.
    /// </summary>
    /// <param name="dte">The selected <see cref="EnvDTE.DTE"/>. Creates a new instance if null.</param>
    /// <exception cref="Exception">An appropriate shell is not installed on the computer.</exception>
    /// <exception cref="Exception">Creating an instance of the shell failed.</exception>
    public TcDte(DTE? dte = null)
    {
        if (InstalledShell is null)
        {
            throw new Exception("No TwinCAT Shell installed");
        }

        if (dte is not null)
        {
            _dte = dte;
            return;
        }
        
        Logger.LogInfo(this, "Create TwinCAT XAE Shell instance ...");
        _dte = Activator.CreateInstance(InstalledShell) as DTE;
        
        if (_dte is null)
        {
            throw new Exception("Creating instance of TwinCAT XAE Shell failed");
        }
    }
    
    /// <summary>
    /// Executes the 'File.SaveAll' command.
    /// </summary>
    public void SaveAll()
    {
        Retry.Invoke(() =>
        {
            _dte?.ExecuteCommand("File.SaveAll");
        });
    }

    /// <summary>
    /// Opens a solution in the current <see cref="DTE"/> instance.
    /// </summary>
    /// <param name="fileName">The path of the solution file.</param>
    public void OpenSolution(string fileName)
    {
        Retry.Invoke(() => { Solution?.Open(fileName); });
    }
    
    /// <summary>
    /// Gets the <see cref="Solution"/> of the current <see cref="DTE"/>.
    /// </summary>
    public Solution? Solution => Retry.Invoke(() => _dte?.Solution);
    
    /// <summary>
    /// Gets the <see cref="Solution.FullName"/> of the current <see cref="DTE"/>.
    /// </summary>
    public string? SolutionFullName => Retry.Invoke(() => Solution?.FullName);
    
    /// <summary>
    /// Gets the <see cref="Solution.FileName"/> of the current <see cref="DTE"/>.
    /// </summary>
    public string? SolutionFileName => Retry.Invoke(() => Solution?.FileName);
    
    /// <summary>
    /// Gets the <see cref="Solution.SolutionBuild"/> of the current <see cref="DTE"/>.
    /// </summary>
    public SolutionBuild? SolutionBuild => Retry.Invoke(() => Solution?.SolutionBuild);
    
    /// <summary>
    /// Gets the <see cref="Events.SolutionEvents"/> of the current <see cref="DTE"/>.
    /// </summary>
    public SolutionEvents? SolutionEvents => Retry.Invoke(() => _dte?.Events.SolutionEvents);
    
    /// <summary>
    /// Gets the <see cref="Events.BuildEvents"/> of the current <see cref="DTE"/>.
    /// </summary>
    public BuildEvents? BuildEvents => Retry.Invoke(() => _dte?.Events.BuildEvents);
    
    /// <summary>
    /// Gets or sets the <see cref="DTE.UserControl"/> of the current <see cref="DTE"/>.
    /// </summary>
    public bool UserControl
    {
        get
        {
            return _dte is not null && Retry.Invoke(() => _dte.UserControl);
        }
        set
        {
            if (_dte is null)
            {
                return;
            }
            Retry.Invoke(() => { _dte.UserControl = value; return _dte.UserControl; });
        }
    }

    /// <summary>
    /// Tries to get the path of the project folder.
    /// </summary>
    /// <returns>The path of the project folder if succeeded, otherwise <see langword="null"/>.</returns>
    public string? GetProjectFolder()
    {
        var project = GetTcProject(_dte);
        return project is null ? null : Retry.Invoke(() => Directory.GetParent(project.FullName)?.FullName);
    }

    /// <summary>
    /// Tries to get <see cref="ITcSysManager15"/> of the current <see cref="DTE"/>.
    /// </summary>
    /// <returns>The <see cref="ITcSysManager15"/> interface if succeeded, otherwise <see langword="null"/>.</returns>
    public ITcSysManager15? GetTcSysManager()
    {
        return Retry.Invoke(() => GetTcProject(_dte)?.Object as ITcSysManager15);
    }
    
    /// <summary>
    /// Tries to get the first <see cref="Project"/> of the given <see cref="DTE"/> that implements <see cref="ITcSysManager15"/>.
    /// </summary>
    /// <param name="dte">The <see cref="DTE"/> to get the project from.</param>
    /// <returns>The first <see cref="Project"/> implementing <see cref="ITcSysManager15"/> if succeeded, otherwise <see langword="null"/>.</returns>
    public static Project? GetTcProject(DTE? dte)
    {
        if (dte is null) return null;
        return Retry.Invoke(() => (
            from Project project in dte.Solution.Projects 
            select project).FirstOrDefault(pro => pro.Object is ITcSysManager15));
    }
    
    /// <summary>
    /// Returns a collection of <see cref="TcDte"/> by querying all running <c>TwinCAT XAE Shell</c> instances
    /// with a valid TwinCAT solution.
    /// </summary>
    public static IEnumerable<TcDte> GetInstances()
    {
        if (InstalledShell is null) yield break;
        if (GetRunningObjectTable(0, out var runningObjectTable) != 0) yield break;
        runningObjectTable.EnumRunning(out var enumMoniker);

        var fetched = IntPtr.Zero;
        var moniker = new IMoniker[1];

        while (enumMoniker.Next(1, moniker, fetched) == 0)
        {
            TcDte? tcDte;
            
            try
            {
                CreateBindCtx(0, out var bindCtx);
                moniker[0].GetDisplayName(bindCtx, null, out var displayName);
                if (!displayName.StartsWith("!TcXaeShell.DTE")) continue;
                if (runningObjectTable.GetObject(moniker[0], out var obj) != 0) continue;
                var dte = (DTE) obj;
                if (dte.Solution.FullName == string.Empty) continue;
                if (GetTcProject(dte) is null) continue;
                tcDte = new TcDte(dte);
            }
            catch (Exception e)
            {
                Logger.LogError(typeof(TcDte), e.Message);
                continue;
            }
            
            yield return tcDte;
        }
    }

    [DllImport("ole32.dll")]
    private static extern void CreateBindCtx(int reserved, out IBindCtx bindCtx);

    [DllImport("ole32.dll")]
    private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable runningObjectTable);
}