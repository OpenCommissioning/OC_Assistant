using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using EnvDTE;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using TCatSysManagerLib;
using Process = System.Diagnostics.Process;

namespace OC.Assistant.PnGenerator;

public class Control(string scannerTool)
{
    private Settings _settings;

    /// <summary>
    /// Starts capturing.
    /// </summary>
    internal void StartCapture(Settings settings)
    {
        if (BusyState.IsSet) return;
        _settings = settings;
            
        if (_settings.PnName == "")
        {
            Logger.LogError(this, "Empty profinet name");
            return;
        }
            
        if (_settings.Adapter is null)
        {
            Logger.LogError(this, "No adapter selected");
            return;
        }
        
        if (_settings.HwFilePath is null)
        {
            Logger.LogError(this, "TIA aml file not specified");
            return;
        }
        
        DteSingleThread.Run(dte =>
        {
            RunScanner();
            ImportPnDevice(dte);
        });
    }

    /// <summary>
    /// Runs the scanner application.
    /// </summary>
    private void RunScanner()
    {
        Logger.LogInfo(this, $"Running {scannerTool}...");
            
        var filePath = $"{AppData.Path}\\{_settings.PnName}.xti";
        
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "cmd",
            Arguments = $"/c {scannerTool} -d \"{_settings.Adapter?.Id}\" -o \"{filePath}\" --aml-file \"{_settings.HwFilePath}\" --gsd-path \"{_settings.GsdFolderPath}\""
            //RedirectStandardOutput = true,
            //RedirectStandardError = true,
            //CreateNoWindow = true
        };
        
        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Logger.LogInfo(this, e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Logger.LogError(this, e.Data);
            }
        };
        
        try
        {
            process.Start();
            //process.BeginOutputReadLine();
            //process.BeginErrorReadLine();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Logger.LogError(this, $"{scannerTool} has stopped with exit code 0x{process.ExitCode:x8}");
                return;
            }
            Logger.LogInfo(this, $"{scannerTool} has finished");
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }

    /// <summary>
    /// Imports a xti-file.
    /// </summary>
    private void ImportPnDevice(DTE dte)
    {
        //No file found
        var xtiFilePath = $"{AppData.Path}\\{_settings.PnName}.xti";
        if (!File.Exists(xtiFilePath))
        {
            Logger.LogInfo(this, "Nothing created");
            return;
        }

        //File is empty
        if (File.ReadAllText(xtiFilePath) == string.Empty)
        {
            Logger.LogInfo(this, "Nothing created");
            File.Delete(xtiFilePath);
            return;
        }
        
        var tcSysManager = dte.GetTcSysManager();
        tcSysManager?.SaveProject();
            
        //Import and delete xti file 
        Logger.LogInfo(this, $"Import {xtiFilePath}...");
        var tcPnDevice = tcSysManager?.UpdateIoDevice(_settings.PnName, xtiFilePath);
        File.Delete(xtiFilePath);
            
        UpdateTcPnDevice(tcPnDevice);
        
        if (tcSysManager?.TryGetPlcProject() is {} plcProjectItem)
        {
            Logger.LogInfo(this, "Create HiL structure...");
            Generator.Generators.Hil.Update(dte, plcProjectItem);
        }

        Logger.LogInfo(this, "Finished");
    }
        
    /// <summary>
    /// Updates the PN-Device.
    /// </summary>
    private void UpdateTcPnDevice(ITcSmTreeItem? tcPnDevice)
    {
        if (tcPnDevice is null) return;
        
        // Add the *.aml filename for information
        if (!string.IsNullOrEmpty(_settings.HwFilePath)) tcPnDevice.Comment = _settings.HwFilePath;
            
        // Set the network adapter
        var deviceDesc = $"{_settings.Adapter?.Name} ({_settings.Adapter?.Description})";
        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            if ($"{adapter.Name} ({adapter.Description})" != deviceDesc) continue;
            var pnDevice = XDocument.Parse(tcPnDevice.ProduceXml());
            var pnp = pnDevice.Root?.Element("DeviceDef")?.Element("AddressInfo")?.Element("Pnp");

            if (pnp?.Element("DeviceDesc") is { } desc)
            {
                desc.Value = deviceDesc;
            }
            
            if (pnp?.Element("DeviceName") is { } name)
            {
                name.Value = $@"\DEVICE\{adapter.Id}";
            }
            
            if (pnp?.Element("DeviceData") is { } data)
            {
                data.Value = adapter.GetPhysicalAddress().ToString();
            }
            
            tcPnDevice.ConsumeXml(pnDevice.ToString());
            return;
        }
    }
}