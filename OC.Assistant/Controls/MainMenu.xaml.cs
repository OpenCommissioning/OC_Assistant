using System.Windows.Controls;

namespace OC.Assistant.Controls;

public partial class MainMenu
{
    public MainMenu()
    {
        InitializeComponent();
        MenuContentAdded += content =>
        {
            Dispatcher.Invoke(() =>
            {
                if (content is not MenuItem menuItem) return;
                Items.Add(menuItem);
            });
        };
    }
    
    private static event Action<object>? MenuContentAdded;
    
    public static void AddMenuContent(object content)
    {
        MenuContentAdded?.Invoke(content);
    }
}