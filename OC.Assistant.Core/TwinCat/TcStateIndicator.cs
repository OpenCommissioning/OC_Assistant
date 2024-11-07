using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OC.Assistant.Core.TwinCat;

public class TcStateIndicator : Border
{
    private readonly Label _label = new();
    
    protected TcStateIndicator()
    {
        Width = 50;
        VerticalAlignment = VerticalAlignment.Center;
        CornerRadius = (CornerRadius)Application.Current.Resources["ControlCornerRadius"];
        
        _label.VerticalAlignment = VerticalAlignment.Center;
        _label.HorizontalAlignment = HorizontalAlignment.Center;
        _label.Foreground = Application.Current.Resources["ForegroundBaseBrush"] as SolidColorBrush;
        Loaded += (_, _) => Child = _label;
        
        IndicateDisconnected();
    }

    protected void IndicateDisconnected()
    {
        Dispatcher.Invoke(() =>
        {
            Background = Application.Current.Resources["White4Brush"] as SolidColorBrush;
            _label.Content = "Offline";
        });
    }

    protected void IndicateRunMode()
    {
        Dispatcher.Invoke(() =>
        {
            Background = Application.Current.Resources["Green1Brush"] as SolidColorBrush;
            _label.Content = "Run";
        });
    }

    protected void IndicateConfigMode()
    {
        Dispatcher.Invoke(() =>
        {
            Background = Application.Current.Resources["AccentBrush"] as SolidColorBrush;
            _label.Content = "Config";
        });
    }
}