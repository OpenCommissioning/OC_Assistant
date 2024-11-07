using System.Windows;

namespace OC.Assistant.Generator;

public partial class Menu
{
    private readonly Control _control = new ();
    
    public Menu()
    {
        InitializeComponent();
        _control.IsEnabledChanged += (_, e) => IsEnabled = (bool)e.NewValue;
    }

    private void CreateProjectOnClick(object sender, RoutedEventArgs e)
    {
        _control.CreateProjectAndHil();
    }
    
    private void CreateTaskOnClick(object sender, RoutedEventArgs e)
    {
        _control.CreateTask();
    }
    
    private void CreatePluginsOnClick(object sender, RoutedEventArgs e)
    {
        _control.CreatePlugins();
    }
    
    private void SettingsOnClick(object sender, RoutedEventArgs e)
    {
        var settings = new Settings();

        if (Theme.MessageBox
                .Show("Project Settings", settings, MessageBoxButton.OKCancel, MessageBoxImage.None) ==
            MessageBoxResult.OK)
        {
            settings.Save();
        }
    }
}