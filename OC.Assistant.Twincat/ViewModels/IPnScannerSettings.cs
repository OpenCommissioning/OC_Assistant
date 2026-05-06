using System.Net.NetworkInformation;

namespace OC.Assistant.Twincat.ViewModels;

public interface IPnScannerSettings
{
    string PnName { get; }
    NetworkInterface? Adapter { get;  }
    string? HwFile { get;  }
    string? GsdFolder { get; }
}