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
       
    private void SelectAmlFileOnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "TIA aml export (*.aml)|*.aml"
        };

        if (openFileDialog.ShowDialog() != true) return;
        _hwFilePath = openFileDialog.FileName;
        HwFileTextBlock.Text = _hwFilePath;
    }
}