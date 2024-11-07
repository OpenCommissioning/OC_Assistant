using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using OC.Assistant.Sdk;

namespace OC.Assistant.PnGenerator;

/// <summary>
/// Dropdown for network adapters, filtered by 'twincat-intel'
/// </summary>
internal class AdapterDropdown : ComboBox
{
    public AdapterDropdown()
    {
        Style = Application.Current.Resources["DefaultComboBoxStyle"] as Style;
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