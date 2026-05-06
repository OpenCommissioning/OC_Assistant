using Avalonia.Controls;

namespace OC.Assistant.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        var settings = App.Settings;
        Height = settings.Height < 100 ? 400 : settings.Height;
        Width = settings.Width < 150 ? 600 : settings.Width;
        Position = new Avalonia.PixelPoint(settings.Left, settings.Top);
    }
    
    private void WindowOnClosing(object? sender, WindowClosingEventArgs e)
    {
        var settings = App.Settings;
        settings.Height = (int)Height;
        settings.Width = (int)Width;
        settings.Left = Position.X;
        settings.Top = Position.Y;
        settings.Write();
    }
}