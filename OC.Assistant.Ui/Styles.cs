using Avalonia.Markup.Xaml.Styling;

namespace OC.Assistant.Ui;

/// <summary>
/// The styles of the theme. Include this in the application styles.
/// </summary>
public class Styles : StyleInclude
{
    /// <summary>
    /// Creates the <see cref="StyleInclude"/> pointing at the package styles.
    /// </summary>
    public Styles() : base(new Uri("avares://OC.Assistant.Ui/"))
    {
        Source = new Uri("avares://OC.Assistant.Ui/Styles.axaml");
    }
}