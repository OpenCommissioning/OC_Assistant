using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

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
        GsdFolderPath = _gsdFolderPath,
        Duration = int.TryParse(Duration.Text, out var result) ? result : 60
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
    
    private void SelectGsdFolderOnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new FolderBrowserDialog
        {
            Description = "GSDML folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        _gsdFolderPath = dialog.SelectedPath;
        GsdFolderTextBlock.Text = _gsdFolderPath;
    }
}