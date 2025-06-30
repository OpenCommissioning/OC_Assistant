using System.Windows.Controls;
using System.Net.NetworkInformation;
using OC.Assistant.Sdk;

namespace OC.Assistant.PnGenerator;

public partial class AdapterDropdown
{
    public AdapterDropdown()
    {
        InitializeComponent();

        try
        {
            var defaultItem = SelectedItem;
            Items.Clear();
                
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!adapter.Description.Contains("twincat-intel", StringComparison.CurrentCultureIgnoreCase)) continue;
                    
                var comboBoxItem = new ComboBoxItem
                {
                    Content = $"{adapter.Name} ({adapter.Description})",
                    Tag = adapter
                };
                    
                Items.Add(comboBoxItem);
            }

            if (Items.Count == 0) Items.Add(defaultItem);
            SelectedIndex = 0;
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }
}