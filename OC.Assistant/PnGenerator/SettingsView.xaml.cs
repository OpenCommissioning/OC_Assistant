using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Microsoft.Win32;
using OC.Assistant.Sdk;

namespace OC.Assistant.PnGenerator;

public partial class SettingsView
{
    private string? _hwFilePath;
    
    public SettingsView()
    {
        InitializeComponent();
    }
    
    public Settings Settings => new()
    {
        PnName = PnName.Text,
        Adapter = SelectedAdapter,
        HwFilePath = _hwFilePath
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
       
    private void SelectHwFileOnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "hwml-Document (*.hwml)|*.hwml",
            RestoreDirectory = true
        };

        if (openFileDialog.ShowDialog() != true) return;
        try
        {
            _ = XDocument.Load(openFileDialog.FileName);
            _hwFilePath = openFileDialog.FileName;
            HwFileTextBlock.Text = _hwFilePath;
        }
        catch (Exception ex)
        {
            Logger.LogError(this, "Error reading Hardware file.");
            Logger.LogError(this, ex.Message);
        }
    }
}