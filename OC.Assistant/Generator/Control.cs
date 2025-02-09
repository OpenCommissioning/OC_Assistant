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

    public override void OnConnect()
    {
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
        IsBusy = true;
        Logger.LogInfo(this, "Project will be updated. Please wait...");

        SingleThread.Run(() =>
        {
            try
            {
                Core.XmlFile.Instance.Reload();
                var dte = TcDte.GetInstance(SolutionFullName);
                if (dte is null) return;
                var plcProjectItem = GetPlcProject(dte);
                if (plcProjectItem is null) return;
                Generators.Hil.Update(dte, plcProjectItem);
                Generators.Project.Update(plcProjectItem);
                IsBusy = false;
                Logger.LogInfo(this, "Project update successful.");
            }
            catch (Exception e)
            {
                Logger.LogError(this, $"Error updating project: {e.Message}");
                IsBusy = false;
            }
        });
    }
    
    public void CreatePlugins()
    {
        IsBusy = true;
        Logger.LogInfo(this, "Project will be updated. Please wait...");

        SingleThread.Run(() =>
        {
            try
            {
                Core.XmlFile.Instance.Reload();
                var plcProjectItem = GetPlcProject();
                if (plcProjectItem is null) return;
                Generators.Sil.UpdateAll(plcProjectItem);
                IsBusy = false;
                Logger.LogInfo(this, "Project update successful.");
            }
            catch (Exception e)
            {
                Logger.LogError(this, $"Error updating project: {e.Message}");
                IsBusy = false;
            }
        });
    }

    private void CreatePlugin(string name, bool delete)
    {
        IsBusy = true;
        Logger.LogInfo(this, "Project will be updated. Please wait...");

        SingleThread.Run(() =>
        {
            try
            {
                var plcProjectItem = GetPlcProject();
                if (plcProjectItem is null) return;
                Generators.Sil.Update(plcProjectItem, name, delete);
                IsBusy = false;
                Logger.LogInfo(this, "Project update successful.");
            }
            catch (Exception e)
            {
                Logger.LogError(this, $"Error updating project: {e.Message}");
                IsBusy = false;
            }
        });
    }

    private void CreateProject()
    {
        IsBusy = true;
        Logger.LogInfo(this, "Project will be updated. Please wait...");

        SingleThread.Run(() =>
        {
            try
            {
                var plcProjectItem = GetPlcProject();
                if (plcProjectItem is null) return;
                Generators.Project.Update(plcProjectItem);
                IsBusy = false;
                Logger.LogInfo(this, "Project update successful.");
            }
            catch (Exception e)
            {
                Logger.LogError(this, $"Error updating project: {e.Message}");
                IsBusy = false;
            }
        });
    }

    public void CreateTask()
    {
        IsBusy = true;
        
        SingleThread.Run(() =>
        {
            var tcSysManager = TcDte.GetInstance(SolutionFullName).GetTcSysManager();
            tcSysManager?.SaveProject();
            if (Generators.Task.CreateVariables(tcSysManager))
            {
                Logger.LogInfo(this, "Task variables have been updated.");
            }
            IsBusy = false;
        });
    }
        
    private ITcSmTreeItem? GetPlcProject(DTE? dte = null)
    {
        dte ??= TcDte.GetInstance(SolutionFullName);
        var tcSysManager = dte?.GetTcSysManager();
        tcSysManager?.SaveProject();
        var plcProjectItem = tcSysManager?.TryGetPlcProject();
        if (plcProjectItem is not null) return plcProjectItem;
        Logger.LogError(this, "No Plc project found");
        IsBusy = false;
        return null;
    }
}