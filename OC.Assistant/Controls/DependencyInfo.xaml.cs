using System.Diagnostics;
using System.Windows;

namespace OC.Assistant.Controls;

public partial class DependencyInfo
{
    private string? _url;
    
    public DependencyInfo(Type type)
    {
        InitializeComponent();
        NameLabel.Content = type.Assembly.GetName().Name;
        VersionLabel.Content = type.Assembly.GetName().Version?.ToString();
    }
    
    public DependencyInfo(string? name, string? version = null)
    {
        InitializeComponent();
        NameLabel.Content = name;
        VersionLabel.Content = version;
    }
    
    public string? Url
    {
        get => _url;
        set
        {
            if (value is null)
            {
                UrlButton.Visibility = Visibility.Collapsed;
                return;
            }
            UrlButton.Visibility = Visibility.Visible;
            _url = value;
            UrlButton.ToolTip = value;
        }
    }
    
    public string? UrlName
    {
        set => UrlButton.Content = value;
    }
    

    private void UrlButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (Url is null) return;
        
        Process.Start(new ProcessStartInfo
        {
            FileName = Url,
            UseShellExecute = true
        });
    }
}