namespace OC.Assistant.Theme;

/// <summary>
/// The resources of the theme. Add this to the application resources.
/// </summary>
public class ResourceDictionary : System.Windows.ResourceDictionary
{
    /// <summary>
    /// Creates the <see cref="Uri"/> of the theme resources.
    /// </summary>
    public ResourceDictionary()
    {
        Source = new Uri("pack://application:,,,/OC.Assistant.Theme;component/ThemeResources.xaml");
    }
}