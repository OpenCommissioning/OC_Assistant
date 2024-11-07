using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OC.Assistant.Theme.Internals;

internal class RectanglePlacement : IMultiValueConverter
{
    public Thickness Margin { get; set; }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var margin = Margin;
            var topLeft = new Point(margin.Left, margin.Top);
            var bottomRight = new Point((double) values[0] - margin.Right, (double) values[1] - margin.Bottom);
            return new Rect(topLeft, bottomRight);
        }
        catch
        {
            return Rect.Empty;
        }
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return null;
    }
}