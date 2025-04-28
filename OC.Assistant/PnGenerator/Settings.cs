﻿using System.Net.NetworkInformation;

namespace OC.Assistant.PnGenerator;

public struct Settings
{
    public string PnName { get; set; }
    public NetworkInterface? Adapter { get; set; }
    public string? HwFilePath { get; set; }
    public string? GsdFolderPath { get; set; }
}