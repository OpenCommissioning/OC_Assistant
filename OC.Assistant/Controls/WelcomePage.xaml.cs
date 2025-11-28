using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OC.Assistant.Sdk;

namespace OC.Assistant.Controls;

public partial class WelcomePage
{
    public WelcomePage()
    {
        InitializeComponent();
        AppControl.Instance.Connected += _ => Visibility = Visibility.Hidden;
        AppControl.Instance.Disconnected += () => Visibility = Visibility.Visible;
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
            AppControl.Instance.Connect(openFileDialog.FileName);
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

        AppControl.Instance.Connect(safeFileDialog.FileName);
    }
}