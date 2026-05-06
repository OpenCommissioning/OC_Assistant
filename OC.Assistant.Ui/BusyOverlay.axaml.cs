using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace OC.Assistant.Ui;

/// <summary>
/// Represents an animated overlay displayed when the application is busy.
/// </summary>
public partial class BusyOverlay : Grid
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    
    /// <summary>
    /// Identifies the <see cref="IsVisible"/> styled property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<BusyOverlay, bool>(nameof(BusyOverlay));

    /// <summary>
    /// Gets or sets a value indicating whether the BusyOverlay is visible and animating.
    /// </summary>
    public new bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <summary>
    /// Creates a new instance of <see cref="BusyOverlay"/>.
    /// </summary>
    public BusyOverlay()
    {
        InitializeComponent();
        base.IsVisible = false;
        IsVisibleProperty.Changed.AddClassHandler<BusyOverlay>(IsVisibleOnChanged);
        var spinnerRotation = SpinnerPath.RenderTransform as RotateTransform;
        _timer.Tick += (_, _) => { spinnerRotation?.Angle = (spinnerRotation.Angle + 8) % 360; };
    }
    
    private void IsVisibleOnChanged(BusyOverlay busyOverlay, AvaloniaPropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            base.IsVisible = e.NewValue is true;
                
            if (!base.IsVisible)
            {
                _timer.Stop();
                return;
            }

            if (_timer.IsEnabled) return;
            _timer.Start();
        });
    }
}