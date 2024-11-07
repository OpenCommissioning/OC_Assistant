using System.Windows;
using System.Windows.Media;

namespace OC.Assistant.Theme;

public partial class BusyOverlay
{
    private readonly RotateTransform _rotateTransform = new ();
    
    public BusyOverlay()
    {
        InitializeComponent();
        Size = 100;
    }

    public void SetState(bool isBusy)
    {
        Dispatcher.Invoke(() =>
        {
            if (!isBusy)
            {
                Visibility = Visibility.Hidden;
                return;
            }
            if (Visibility == Visibility.Visible) return;
            Visibility = Visibility.Visible;
            StartRotate();
        });
    }

    public double Size
    {
        set
        {
            Grid.Height = value;
            Grid.Width = value;
            Grid.Margin = new Thickness(value, value, 0, 0);
            Label.Margin = new Thickness(-value / 4, -value / 4, 0, 0);
            Label.FontSize = value / 2;
        }
    }
    
    private void StartRotate()
    {
        Task.Run(() =>
        {
            while (Visibility == Visibility.Visible)
            {
                Dispatcher.Invoke((Action) (() =>
                {
                    _rotateTransform.Angle += 3.0;
                    Grid.RenderTransform = _rotateTransform;
                }));
                Thread.Sleep(16);
            }
        });
    }
}