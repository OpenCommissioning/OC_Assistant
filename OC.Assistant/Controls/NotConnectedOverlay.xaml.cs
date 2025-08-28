using System.Windows;
using OC.Assistant.Core;

namespace OC.Assistant.Controls;

public partial class NotConnectedOverlay
{
    public NotConnectedOverlay()
    {
        InitializeComponent();
        ProjectState.Events.Connected += OnConnect;
        ProjectState.Events.Disconnected += OnDisconnect;
        ContentAdded += uiElement =>
        {
            Dispatcher.Invoke(() =>
            {
                StackPanel.Children.Add(uiElement);
            });
        };
    }

    private void OnConnect(string solutionFullName) => Visibility = Visibility.Hidden;
    
    private void OnDisconnect() => Visibility = Visibility.Visible;

    private static event Action<UIElement>? ContentAdded;
    
    public static void AddContent(UIElement uiElement)
    {
        ContentAdded?.Invoke(uiElement);
    }
}