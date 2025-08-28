using System.Windows;

namespace OC.Assistant.Controls;

internal partial class FileMenu
{
    public FileMenu()
    {
        InitializeComponent();
        ContentAdded += uiElement =>
        {
            Dispatcher.Invoke(() =>
            {
                Items.Insert(0, uiElement);
            });
        };
    }
    
    private void ExitOnClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    
    private static event Action<UIElement>? ContentAdded;
    
    public static void AddContent(UIElement uiElement)
    {
        ContentAdded?.Invoke(uiElement);
    }
}