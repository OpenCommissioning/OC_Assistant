﻿using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;

namespace OC.Assistant.PnGenerator;

public partial class SettingsView
{
    private string? _hwFilePath;
    private string? _gsdFolderPath;
    
    public SettingsView()
    {
        InitializeComponent();
    }
    
    public Settings Settings => new()
    {
        PnName = PnName.Text,
        Adapter = SelectedAdapter,
        HwFilePath = _hwFilePath,
        GsdFolderPath = _gsdFolderPath
    };
    
    private NetworkInterface? SelectedAdapter
    {
        get
        {
            if (AdapterDropdown.SelectedIndex == -1) return null;
            var selection = (ComboBoxItem) AdapterDropdown.SelectedItem;
            return selection?.Tag as NetworkInterface;
        }
    }
       
    private void SelectAmlFileOnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "TIA aml export (*.aml)|*.aml"
        };

        if (openFileDialog.ShowDialog() != true) return;
        _hwFilePath = openFileDialog.FileName;
        HwFileTextBlock.Text = _hwFilePath;
    }
    
    private void SelectGsdFolderOnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "GSDML folder"
        };

        if (dialog.ShowDialog() != true) return;
        _gsdFolderPath = dialog.FolderName;
        GsdFolderTextBlock.Text = _gsdFolderPath;
    }
}