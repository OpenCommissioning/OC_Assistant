using System.Xml.Linq;
using EnvDTE;
using OC.Assistant.Core;
using OC.Assistant.Core.TwinCat;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Generator;

public class Control : ControlBase
{
    public Control()
    {
        ApiLocal.Interface.ConfigReceived += ApiOnConfigReceived;
        ApiLocal.Interface.SilUpdate += ApiOnSilUpdate;
    }

    private void ApiOnSilUpdate(string name, bool delete)
    {
        CreatePlugin(name, delete);
    }

    private void ApiOnConfigReceived(XElement config)
    {
        XmlFile.ClientUpdate(config);
        CreateProject();
    }
    
    private string? SolutionFullName { get; set; }

    public override void OnConnect(string solutionFullName)
    {
        SolutionFullName = solutionFullName;
    }
        
    public override void OnDisconnect()
    {
    }

    public override void OnTcStopped()
    {
    }

    public override void OnTcStarted()
    {
    }
    
    public void CreateProjectAndHil()
    {
        SingleThread.Run(() =>
        {
            if (GetDte() is not {} dte) return;
            if (GetPlcProject(dte) is not {} plcProjectItem) return;
            try
            {
                Core.XmlFile.Instance.Reload();
                Generators.Hil.Update(dte, plcProjectItem);
                Generators.Project.Update(plcProjectItem);
                Logger.LogInfo(this, "Project update successful.");
            }
            catch (Exception e)
            {
                Logger.LogError(this, $"Error updating project: {e.Message}");
            }
            finally
            {
                FinalizeDte(dte);
            }
        });
    }
    
    public void CreatePlugins()
    {
        SingleThread.Run(() =>
        {
            if (GetDte() is not {} dte) return;
            if (GetPlcProject(dte) is not {} plcProjectItem) return;
            try
            {
                Core.XmlFile.Instance.Reload();
                Generators.Sil.UpdateAll(plcProjectItem);
                Logger.LogInfo(this, "Project update successful.");
            }
            catch (Exception e)
            {
                Logger.LogError(this, $"Error updating project: {e.Message}");
            }
            finally
            {
                FinalizeDte(dte);
            }
        });
    }

    private void CreatePlugin(string name, bool delete)
    {
        SingleThread.Run(() =>
        {
            if (GetDte() is not {} dte) return;
            if (GetPlcProject(dte) is not {} plcProjectItem) return;
            try
            {
                Generators.Sil.Update(plcProjectItem, name, delete);
                Logger.LogInfo(this, "Project update successful.");
            }
            catch (Exception e)
            {
                Logger.LogError(this, $"Error updating project: {e.Message}");
            }
            finally
            {
                FinalizeDte(dte);
            }
        });
    }

    private void CreateProject()
    {
        SingleThread.Run(() =>
        {
            if (GetDte() is not {} dte) return;
            if (GetPlcProject(dte) is not {} plcProjectItem) return;
            try
            {
                Generators.Project.Update(plcProjectItem);
                Logger.LogInfo(this, "Project update successful.");
            }
            catch (Exception e)
            {
                Logger.LogError(this, $"Error updating project: {e.Message}");
            }
            finally
            {
                FinalizeDte(dte);
            }
        });
    }

    public void CreateTask()
    {
        IsBusy = true;
        
        SingleThread.Run(() =>
        {
            if (GetDte() is not {} dte) return;

            try
            {
                Generators.Task.CreateVariables(dte);
                Logger.LogInfo(this, "Project update successful.");
            }
            catch (Exception e)
            {
                Logger.LogError(this, $"Error updating project: {e.Message}");
            }
            finally
            {
                FinalizeDte(dte);
            }
        });
    }
    
    private DTE? GetDte()
    {
        IsBusy = true;
        Logger.LogInfo(this, "Project will be updated. Please wait...");
        if (TcDte.GetInstance(SolutionFullName) is {} dte) return dte;
        IsBusy = false;
        return null;
    }
    
    private ITcSmTreeItem? GetPlcProject(DTE? dte)
    {
        var tcSysManager = dte?.GetTcSysManager();
        tcSysManager?.SaveProject();
        if (tcSysManager?.TryGetPlcProject() is {} plcProjectItem) return plcProjectItem;
        Logger.LogError(this, "No Plc project found");
        IsBusy = false;
        return null;
    }

    private void FinalizeDte(DTE? dte)
    {
        dte?.Finalize();
        IsBusy = false;
    }
}