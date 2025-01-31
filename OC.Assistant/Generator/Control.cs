﻿using System.Xml.Linq;
using EnvDTE;
using OC.Assistant.Core;
using OC.Assistant.Core.TwinCat;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Generator;

public class Control : ControlBase
{
    private BuildEvents? _buildEvents;

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
        _buildEvents = TcDte?.BuildEvents;
        if (_buildEvents is null) return;
        _buildEvents.OnBuildDone += BuildEvents_OnBuildDone;
    }
        
    public override void OnDisconnect()
    {
        if (_buildEvents is null) return;
        try
        {
            _buildEvents.OnBuildDone -= BuildEvents_OnBuildDone;
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message, true);
        }
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

        Task.Run(() =>
        {
            try
            {
                Core.XmlFile.Instance.Reload();
                var plcProjectItem = GetPlcProject();
                if (plcProjectItem is null) return;
                Generators.Hil.Update(this, plcProjectItem);
                Generators.Project.Update(plcProjectItem);
                TcSysManager?.SaveProject();
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

        Task.Run(() =>
        {
            try
            {
                Core.XmlFile.Instance.Reload();
                var plcProjectItem = GetPlcProject();
                if (plcProjectItem is null) return;
                Generators.Sil.UpdateAll(plcProjectItem);
                TcSysManager?.SaveProject();
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

        Task.Run(() =>
        {
            try
            {
                var plcProjectItem = GetPlcProject();
                if (plcProjectItem is null) return;
                Generators.Sil.Update(plcProjectItem, name, delete);
                TcSysManager?.SaveProject();
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

        Task.Run(() =>
        {
            try
            {
                var plcProjectItem = GetPlcProject();
                if (plcProjectItem is null) return;
                Generators.Project.Update(plcProjectItem);
                TcSysManager?.SaveProject();
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
        
        Task.Run(() =>
        {
            TcSysManager?.SaveProject();
            if (Generators.Task.CreateVariables(TcSysManager))
            {
                Logger.LogInfo(this, "Task variables have been updated.");
            }
            TcSysManager?.SaveProject();
            IsBusy = false;
        });
    }
        
    private ITcSmTreeItem? GetPlcProject()
    {
        TcSysManager?.SaveProject();
        var plcProjectItem = TcSysManager?.TryGetPlcProject();
        if (plcProjectItem is not null) return plcProjectItem;
        Logger.LogError(this, "No Plc project found");
        IsBusy = false;
        return null;
    }

    private void BuildEvents_OnBuildDone(vsBuildScope scope, vsBuildAction action)
    {
        if (!XmlFile.XmlBase.TaskAutoUpdate) return;
        if (action == vsBuildAction.vsBuildActionClean) return;
        if (IsBusy) return;
        if (TcDte?.SolutionBuild?.LastBuildInfo != 0) return;
            
        IsBusy = true;
        CreateTask();
    }
}