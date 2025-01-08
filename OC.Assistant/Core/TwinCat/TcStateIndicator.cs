using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OC.Assistant.Core.TwinCat;

public class TcStateIndicator : StackPanel
{
    private readonly Border _stateBorder = new();
    private readonly Label _stateLabel = new();
    private readonly Border _netIdBorder = new();
    private readonly Label _nedIdLabel = new();
    private readonly Border _solutionBorder = new();
    private readonly Label _solutionLabel = new();
    
    protected TcStateIndicator()
    {
        _stateLabel.VerticalAlignment = VerticalAlignment.Center;
        _stateLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _stateLabel.Foreground = Application.Current.Resources["ForegroundBaseBrush"] as SolidColorBrush;
        
        _stateBorder.Width = 50;
        _stateBorder.VerticalAlignment = VerticalAlignment.Center;
        _stateBorder.CornerRadius = (CornerRadius)Application.Current.Resources["ControlCornerRadius"];
        _stateBorder.Child = _stateLabel;
        
        _nedIdLabel.VerticalAlignment = VerticalAlignment.Center;
        _nedIdLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _nedIdLabel.Foreground = Application.Current.Resources["ForegroundBaseBrush"] as SolidColorBrush;
        
        _netIdBorder.VerticalAlignment = VerticalAlignment.Center;
        _netIdBorder.CornerRadius = (CornerRadius)Application.Current.Resources["ControlCornerRadius"];
        _netIdBorder.Background = Application.Current.Resources["White4Brush"] as SolidColorBrush;
        _netIdBorder.Margin = new Thickness(0, 0, 3, 0);
        _netIdBorder.Padding = new Thickness(5, 0, 5, 0);
        _netIdBorder.Child = _nedIdLabel;
        
        _solutionLabel.VerticalAlignment = VerticalAlignment.Center;
        _solutionLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _solutionLabel.Foreground = Application.Current.Resources["ForegroundBaseBrush"] as SolidColorBrush;
        
        _solutionBorder.VerticalAlignment = VerticalAlignment.Center;
        _solutionBorder.CornerRadius = (CornerRadius)Application.Current.Resources["ControlCornerRadius"];
        _solutionBorder.Background = Application.Current.Resources["White4Brush"] as SolidColorBrush;
        _solutionBorder.Margin = new Thickness(0, 0, 3, 0);
        _solutionBorder.Padding = new Thickness(5, 0, 5, 0);
        _solutionBorder.Child = _solutionLabel;
        
        Orientation = Orientation.Horizontal;
        Children.Add(_solutionBorder);
        Children.Add(_netIdBorder);
        Children.Add(_stateBorder);
        
        IndicateDisconnected();
    }

    protected void SetSolutionPath(string? path)
    {
        Dispatcher.Invoke(() =>
        {
            _solutionLabel.Content = path;
        });
    }

    protected void IndicateDisconnected()
    {
        Dispatcher.Invoke(() =>
        {
            _stateBorder.Background = Application.Current.Resources["White4Brush"] as SolidColorBrush;
            _stateLabel.Content = "Offline";
            _nedIdLabel.Content = null;
            _solutionLabel.Content = null;
        });
    }

    protected void IndicateRunMode()
    {
        Dispatcher.Invoke(() =>
        {
            _stateBorder.Background = Application.Current.Resources["Green1Brush"] as SolidColorBrush;
            _stateLabel.Content = "Run";
            _nedIdLabel.Content = Sdk.ApiLocal.Interface.NetId;
        });
    }

    protected void IndicateConfigMode()
    {
        Dispatcher.Invoke(() =>
        {
            _stateBorder.Background = Application.Current.Resources["AccentBrush"] as SolidColorBrush;
            _stateLabel.Content = "Config";
            _nedIdLabel.Content = Sdk.ApiLocal.Interface.NetId;
        });
    }
}