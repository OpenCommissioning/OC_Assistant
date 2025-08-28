using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Generator;

public class Service
{
    public async Task CreateProject()
    {
        var thread = DteSingleThread.Run(tcSysManager =>
        {
            var plcProjectItem = GetPlcProject(tcSysManager);
            XmlFile.Instance.Reload();
            Generators.Hil.Update(tcSysManager, plcProjectItem);
            Generators.Project.Update(plcProjectItem);
            Logger.LogInfo(this, "Project update finished.");
        }, throwExceptions: true);
        
        await Task.Run(() => thread.Join());
    }
    
    public async Task CreatePlugins()
    {
        var thread = DteSingleThread.Run(dte =>
        {
            XmlFile.Instance.Reload();
            Generators.Sil.UpdateAll(GetPlcProject(dte));
            Logger.LogInfo(this, "Project update finished.");
        }, throwExceptions: true);
        
        await Task.Run(() => thread.Join());
    }
    
    public async Task CreateTask()
    {
        var thread = DteSingleThread.Run(tcSysManager =>
        {
            Generators.Task.CreateVariables(tcSysManager);
            Logger.LogInfo(this, "Project update finished.");
        }, throwExceptions: true);
        
        await Task.Run(() => thread.Join());
    }
    
    public async Task CreateTemplate(string name)
    {
        var generator = new Generators.DeviceTemplate
        {
            InputField =
            {
                Text = name
            }
        };
        if (!generator.CheckName()) throw new Exception("Invalid name");
        await Task.Run(() => generator.Create(true).Join());
    }
    
    public async Task GenerateFromConfig(XElement config)
    {
        XmlFile.Instance.Main = config;
        XmlFile.Instance.Save();
        
        var result = DteSingleThread.Run(tcSysManager =>
        {
            Generators.Project.Update(GetPlcProject(tcSysManager));
            Logger.LogInfo(this, "Project update finished.");
        }, throwExceptions: true);
        
        await Task.Run(() => result.Join());
    }
    
    private ITcSmTreeItem GetPlcProject(ITcSysManager15? tcSysManager)
    {
        tcSysManager?.SaveProject();
        if (tcSysManager?.GetPlcProject() is {} plcProjectItem) return plcProjectItem;
        Logger.LogError(this, "No Plc project found");
        throw new Exception("No Plc project found");
    }
}