using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using OC.Assistant.Sdk;

namespace OC.Assistant.Controls;

public partial class VersionCheck
{
    public VersionCheck()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        return;

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Task.Run(async() =>
            {
                try
                {
                    const string api = "https://api.github.com/repos/opencommissioning/oc_assistant/releases/latest";
                    const string url = "https://github.com/opencommissioning/oc_assistant/releases/latest";
                
                    var version = Assembly.GetExecutingAssembly().GetName().Version;
                    Logger.LogInfo(this, $"Current version {version}");
                    var current = $"v{version?.Major}.{version?.Minor}.{version?.Build}";
                
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                    
                    var latest = JsonDocument
                        .Parse(await client.GetStringAsync(api))
                        .RootElement
                        .GetProperty("tag_name")
                        .GetString();
                
                    if (current != latest)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Visibility = Visibility.Visible;
                            Content = $"Release {latest} available!";
                            Click += (_, _) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                        });
                    }
                }
                catch
                {
                    //Logger.LogWarning(this, $"Unable to fetch latest release from GitHub");
                }
            });
        }
    }
}