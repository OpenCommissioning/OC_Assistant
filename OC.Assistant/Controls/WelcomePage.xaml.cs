using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.Controls;

public partial class WelcomePage
{
    public WelcomePage()
    {
        InitializeComponent();
        AppInterface.Instance.Connected += (_, _) => Visibility = Visibility.Hidden;
        AppInterface.Instance.Disconnected += () => Visibility = Visibility.Visible;
        ContentAdded += content =>
        {
            Dispatcher.Invoke(() =>
            {
                var contentControl = new ContentControl
                {
                    Content = content
                };
                StackPanel.Children.Add(contentControl);
            });
        };
    }

    private static event Action<object>? ContentAdded;
    
    public static void AddContent(object content)
    {
        ContentAdded?.Invoke(content);
    }

    private void OpenOnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "OC.Assistant.xml (*.xml)|*.xml",
            RestoreDirectory = true
        };
        
        if (openFileDialog.ShowDialog() == true)
        {
            AppInterface.Instance.Connect(openFileDialog.FileName);
        }
    }

    private void CreateOnClick(object sender, RoutedEventArgs e)
    {
        var safeFileDialog = new SaveFileDialog
        {
            Filter = "OC.Assistant.xml (*.xml)|*.xml",
            FileName = "OC.Assistant.xml",
            RestoreDirectory = true
        };

        if (safeFileDialog.ShowDialog() != true) return;

        try
        {
            if (File.Exists(safeFileDialog.FileName))
            {
                File.Delete(safeFileDialog.FileName);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(this, ex.Message);
        }

        AppInterface.Instance.Connect(safeFileDialog.FileName);
    }
}