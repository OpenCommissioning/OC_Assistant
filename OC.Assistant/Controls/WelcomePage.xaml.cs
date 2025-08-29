using System.IO;
using System.Windows;
using Microsoft.Win32;
using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.Controls;

public partial class WelcomePage
{
    public WelcomePage()
    {
        InitializeComponent();
        ProjectState.Events.Connected += (_, _) => Visibility = Visibility.Hidden;
        ProjectState.Events.Disconnected += () => Visibility = Visibility.Visible;
        ContentAdded += uiElement =>
        {
            Dispatcher.Invoke(() =>
            {
                StackPanel.Children.Add(uiElement);
            });
        };
    }

    private static event Action<UIElement>? ContentAdded;
    
    public static void AddContent(UIElement uiElement)
    {
        ContentAdded?.Invoke(uiElement);
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
            ProjectState.Control.Connect(openFileDialog.FileName);
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

        ProjectState.Control.Connect(safeFileDialog.FileName);
    }
}