using System.Windows;
using System.Windows.Media;

namespace OC.Assistant.Theme;

/// <summary>
/// Represents a <see cref="Grid"/> element to work as an overlay when the application is busy.
/// </summary>
public partial class BusyOverlay
{
    private readonly RotateTransform _rotateTransform = new ();
    
    /// <summary>
    /// Creates a new instance of the <see cref="BusyOverlay"/>.
    /// </summary>
    public BusyOverlay()
    {
        InitializeComponent();
        Size = 100;
    }

    /// <summary>
    /// Sets the busy state.
    /// </summary>
    /// <param name="isBusy"><br/><c>True</c> : The overlay is visible and animated. <br/>
    /// <c>False</c>: The overlay is hidden.</param>
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

    /// <summary>
    /// Sets the size of the animated icon. Default value is 100.
    /// </summary>
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