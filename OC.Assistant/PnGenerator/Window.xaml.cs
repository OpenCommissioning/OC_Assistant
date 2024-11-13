using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Microsoft.Win32;
using OC.Assistant.Sdk;

namespace OC.Assistant.PnGenerator;

internal partial class Window
{
    public Window()
    {
        InitializeComponent();
    }
       
    private Settings _settings;
    public event Action<Settings>? OnStart;
   
    private NetworkInterface? SelectedAdapter
    {
        get
        {
            if (AdapterDropdown.SelectedIndex == -1) return null;
            var selection = (ComboBoxItem)AdapterDropdown.SelectedItem;
            return (NetworkInterface)selection.Tag;
        }
    }
       
    private void StartOnClick(object sender, RoutedEventArgs e)
    {
        _settings.PnName = PnName.Text;
        _settings.Adapter = SelectedAdapter;
        OnStart?.Invoke(_settings);
        Close();
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
            _settings.HwFilePath = openFileDialog.FileName;
            HwFilePath.Text = _settings.HwFilePath;
        }
        catch (Exception ex)
        {
            Logger.LogError(this, "Error reading Hardware file.");
            Logger.LogError(this, ex.Message);
        }
    }
}