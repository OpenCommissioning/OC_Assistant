using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace OC.Assistant.Controls;

/// <summary>
/// Represents a custom message box with an appropriate theme style.
/// </summary>
public partial class Modal
{
    private static event Action<string, UIElement, MessageBoxButton, MessageBoxImage>? ModalShown;
    private static MessageBoxResult _result = MessageBoxResult.None;
    private readonly Storyboard _fadeIn;
    private readonly Storyboard _fadeOut;
    
    /// <summary>
    /// Occurs when the <see cref="Modal"/> is displayed.
    /// </summary>
    public event Action? Shown;
    
    /// <summary>
    /// Occurs when the <see cref="Modal"/> is closed.
    /// </summary>
    public event Action? Closed;
    
    /// <summary>
    /// Shows the modal with the given parameters. 
    /// </summary>
    /// <param name="caption">The caption of the modal.</param>
    /// <param name="content">The content within the modal.</param>
    /// <param name="button">The <see cref="MessageBoxButton"/> of the modal.</param>
    /// <param name="image">The shown image on the left side.</param>
    public static async Task<MessageBoxResult> Show(string caption, UIElement content, MessageBoxButton button, MessageBoxImage image)
    {
        _result = MessageBoxResult.None;
        ModalShown?.Invoke(caption, content, button, image);
        while (_result == MessageBoxResult.None)
        {
            await Task.Delay(10);
        }
        return _result;
    }
    
    /// <summary>
    /// Shows the modal with the given parameters. 
    /// </summary>
    /// <param name="caption">The caption of the modal.</param>
    /// <param name="text">The text within the modal.</param>
    /// <param name="button">The <see cref="MessageBoxButton"/> of the modal.</param>
    /// <param name="image">The shown image on the left side.</param>
    public static async Task<MessageBoxResult> Show(string caption, string text, MessageBoxButton button, MessageBoxImage image)
    {
        var content = new Label { VerticalAlignment = VerticalAlignment.Center, Content = text};
        return await Show(caption, content, button, image);
    }
    
    public Modal()
    {
        InitializeComponent();
        Visibility = Visibility.Collapsed;
        
        _fadeIn = (Storyboard)FindResource("FadeInStoryboard");
        _fadeOut = (Storyboard)FindResource("FadeOutStoryboard");
        _fadeOut.Completed += FadeOutOnCompleted;
        ModalShown += ModalShow;
    }

    private void ModalShow(string caption, UIElement content, MessageBoxButton button, MessageBoxImage image)
    {
        TitleLabel.Text = caption;
        ContentGrid.Children.Add(content);
        SetButtons(button);
        SetImage(image);
        Visibility = Visibility.Visible;
        _fadeIn.Begin(this);
        Shown?.Invoke();
    }

    private void SetButtons(MessageBoxButton button)
    {
        ButtonOk.Visibility = Visibility.Visible;
        ButtonCancel.Visibility = button switch
        {
            MessageBoxButton.OK => Visibility.Collapsed,
            MessageBoxButton.OKCancel => Visibility.Visible,
            _ => Visibility.Collapsed
        };
    }

    private void SetImage(MessageBoxImage image)
    {
        SymbolInformation.Visibility = image == MessageBoxImage.Information ? Visibility.Visible : Visibility.Collapsed;
        SymbolWarning.Visibility = image == MessageBoxImage.Warning ? Visibility.Visible : Visibility.Collapsed;
        SymbolError.Visibility = image == MessageBoxImage.Error ? Visibility.Visible : Visibility.Collapsed;
        SymbolQuestion.Visibility = image == MessageBoxImage.Question ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ButtonOnClick(object sender, RoutedEventArgs e)
    {
        _result = sender.Equals(ButtonOk) ? MessageBoxResult.OK : MessageBoxResult.Cancel;
        _fadeOut.Begin(this);
        Closed?.Invoke();
    }
    
    private void FadeOutOnCompleted(object? sender, EventArgs e)
    {
        Visibility = Visibility.Collapsed;
        ContentGrid.Children.Clear();
    }
}