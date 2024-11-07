namespace OC.Assistant.Theme;

public class ResourceDictionary : System.Windows.ResourceDictionary
{
    public ResourceDictionary()
    {
        Source = new Uri("pack://application:,,,/OC.Assistant.Theme;component/ThemeResources.xaml");
    }
}