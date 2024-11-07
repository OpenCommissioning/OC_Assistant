using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace OC.Assistant.Theme;

public static class WindowStyle
{
    private static bool _themeIsUsed;
    
    public static readonly DependencyProperty UseThemeProperty =
        DependencyProperty.RegisterAttached(
            "UseTheme",
            typeof(bool),
            typeof(WindowStyle),
            new PropertyMetadata(OnUseThemeChanged));
    
    public static void SetUseTheme(Window window, bool value)
    {
        window.SetValue(UseThemeProperty, value);
    }

    private static void OnUseThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        _themeIsUsed = (bool)e.NewValue;
        var window = (Window)d;
        window.SourceInitialized += WindowOnSourceInitialized;
        
        if (_themeIsUsed)
        {
            window.SetResourceReference(FrameworkElement.StyleProperty, "DefaultWindowStyle");
            return;
        }

        window.ClearValue(FrameworkElement.StyleProperty);
    }

    private static void WindowOnSourceInitialized(object? sender, EventArgs e)
    {
        if (sender is not Window window) return;
        var value = _themeIsUsed;
        var handle = new WindowInteropHelper(window).Handle;
        _ = DwmSetWindowAttribute(handle, 20, ref value, Marshal.SizeOf(value));
    }
    
    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr handle, int attr, ref bool attrValue, int attrSize);
}