using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Core.TwinCat;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.PnGenerator;

public class Control(string scannerTool) : ControlBase
{
    private Settings _settings;

    /// <summary>
    /// Starts capturing.
    /// </summary>
    internal void StartCapture(Settings settings)
    {
        if (IsBusy) return;
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
        
        SingleThread.Run(() =>
        {
            IsBusy = true;
            RunScanner();
            ImportPnDevice();
            IsBusy = false;
        });
    }

    /// <summary>
    /// Runs the scanner application.
    /// </summary>
    private void RunScanner()
    {
        var duration = _settings.Duration;
        Logger.LogInfo(this, $"Running {scannerTool} for {duration} seconds...");
            
        var filePath = $"{TcProjectFolder}\\{_settings.PnName}.xti";
        
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "cmd",
            Arguments = $"/c {scannerTool} -d \"{_settings.Adapter?.Id}\" -t {duration} -o \"{filePath}\""
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
    private void ImportPnDevice()
    {
        //Save TwinCAT project first
        var tcSysManager = TcDte.GetInstance(SolutionFullName).GetTcSysManager();
        tcSysManager?.SaveProject();

        //No file found
        var xtiFilePath = $"{TcProjectFolder}\\{_settings.PnName}.xti";
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
        
        //Update xti file if necessary
        if (_settings.HwFilePath is not null)
        {
            new XtiUpdater().Run(xtiFilePath, _settings.HwFilePath);
        }
            
        //Import and delete xti file 
        Logger.LogInfo(this, $"Import {xtiFilePath}...");
        var tcPnDevice = tcSysManager?.UpdateIoDevice(_settings.PnName, xtiFilePath);
        File.Delete(xtiFilePath);
            
        UpdateTcPnDevice(tcPnDevice);
            
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
            if (pnp is null) return;
            pnp.Element("DeviceDesc")!.Value = deviceDesc;
            pnp.Element("DeviceName")!.Value = $"\\DEVICE\\{adapter.Id}";
            pnp.Element("DeviceData")!.Value = adapter.GetPhysicalAddress().ToString();
            tcPnDevice.ConsumeXml(pnDevice.ToString());
            return;
        }
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
}