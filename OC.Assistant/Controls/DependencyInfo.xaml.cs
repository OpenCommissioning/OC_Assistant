using System.Diagnostics;
using System.Windows;

namespace OC.Assistant.Controls;

public partial class DependencyInfo
{
    public DependencyInfo(Type type)
    {
        InitializeComponent();
        NameLabel.Content = type.Assembly.GetName().Name;
        VersionLabel.Content = type.Assembly.GetName().Version?.ToString();
    }

    public string? Url
    {
        get;
        init
        {
            if (value is null)
            {
                UrlButton.Visibility = Visibility.Collapsed;
                return;
            }

            UrlButton.Visibility = Visibility.Visible;
            field = value;
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