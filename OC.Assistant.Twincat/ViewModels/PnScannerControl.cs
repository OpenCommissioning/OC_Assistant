using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using OC.Assistant.Sdk;
using OC.Assistant.Twincat.Automation;
using TCatSysManagerLib;
using Process = System.Diagnostics.Process;

namespace OC.Assistant.Twincat.ViewModels;

public class PnScannerControl(string scannerTool, IPnScannerSettings pnScannerSettings)
{
    /// <summary>
    /// Starts capturing.
    /// </summary>
    internal void StartCapture()
    {
        if (BusyState.IsSet) return;
            
        if (pnScannerSettings.PnName == "")
        {
            Logger.LogError(this, "Empty profinet name");
            return;
        }
            
        if (pnScannerSettings.Adapter is null)
        {
            Logger.LogError(this, "No adapter selected");
            return;
        }
        
        if (pnScannerSettings.HwFile is null)
        {
            Logger.LogError(this, "TIA aml file not specified");
            return;
        }
        
        DteSingleThread.Run(tcSysManager =>
        {
            RunScanner();
            ImportPnDevice(tcSysManager);
        });
    }

    /// <summary>
    /// Runs the scanner application.
    /// </summary>
    private void RunScanner()
    {
        Logger.LogInfo(this, $"Running {scannerTool}...");
            
        var filePath = Path.Combine(AppData.Path, $"{pnScannerSettings.PnName}.xti");
        
        var convertPnNames = pnScannerSettings.ConvertPnNames ? " --convert-pn-names" : string.Empty;
        
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "cmd",
            Arguments = $"/c {scannerTool} -d \"{pnScannerSettings.Adapter?.Id}\" -o \"{filePath}\" --aml-file \"{pnScannerSettings.HwFile}\" --gsd-path \"{pnScannerSettings.GsdFolder}\"{convertPnNames}"
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
    private void ImportPnDevice(ITcSysManager15 tcSysManager)
    {
        //No file found
        var xtiFilePath = Path.Combine(AppData.Path, $"{pnScannerSettings.PnName}.xti");
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
        
        tcSysManager.SaveProject();
            
        //Import and delete xti file 
        Logger.LogInfo(this, $"Import {xtiFilePath}...");
        var tcPnDevice = tcSysManager.UpdateIoDevice(pnScannerSettings.PnName, xtiFilePath);
        File.Delete(xtiFilePath);
            
        UpdateTcPnDevice(tcPnDevice);
        
        if (tcSysManager.GetPlcProject() is {} plcProjectItem)
        {
            Logger.LogInfo(this, "Create HiL structure...");
            HilGenerator.Update(tcSysManager, plcProjectItem);
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
        if (!string.IsNullOrEmpty(pnScannerSettings.HwFile)) tcPnDevice.Comment = pnScannerSettings.HwFile;
            
        // Set the network adapter
        var deviceDesc = $"{pnScannerSettings.Adapter?.Name} ({pnScannerSettings.Adapter?.Description})";
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