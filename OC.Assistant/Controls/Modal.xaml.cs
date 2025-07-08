using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace OC.Assistant.Controls;

/// <summary>
/// Represents a modal.
/// </summary>
public partial class Modal
{
    private static event Action<string, UIElement, MessageBoxButton, MessageBoxImage>? ModalShown;
    private static MessageBoxResult _result = MessageBoxResult.None;
    private readonly DoubleAnimation _fadeIn = new(0, 1, TimeSpan.FromMilliseconds(200));
    private readonly DoubleAnimation _fadeOut = new(1, 0, TimeSpan.FromMilliseconds(200));
    private static bool _isCreated;

    /// <summary>
    /// Shows the modal dialog with the specified parameters.
    /// </summary>
    /// <param name="caption">The title of the modal.</param>
    /// <param name="content">The content displayed within the modal, represented as a <see cref="UIElement"/>.</param>
    /// <param name="button">The <see cref="MessageBoxButton"/> options available in the modal.</param>
    /// <param name="image">The <see cref="MessageBoxImage"/> icon to display in the modal.</param>
    /// <returns>A <see cref="MessageBoxResult"/> indicating the user's interaction with the modal.</returns>
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
    /// Shows the modal dialog with the specified parameters.
    /// </summary>
    /// <param name="caption">The title of the modal.</param>
    /// <param name="text">The text displayed within the modal.</param>
    /// <param name="button">The <see cref="MessageBoxButton"/> options available in the modal.</param>
    /// <param name="image">The <see cref="MessageBoxImage"/> icon to display in the modal.</param>
    /// <returns>A <see cref="MessageBoxResult"/> indicating the user's interaction with the modal.</returns>
    public static async Task<MessageBoxResult> Show(string caption, string text, MessageBoxButton button, MessageBoxImage image)
    {
        var content = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center, 
            TextWrapping = TextWrapping.Wrap,
            Text = text
        };
        return await Show(caption, content, button, image);
    }
    
    /// <summary>
    /// Creates a new instance of the <see cref="Modal"/>.
    /// </summary>
    public Modal()
    {
        if (_isCreated)
        {
            throw new InvalidOperationException("Modal can only be created once");
        }
        _isCreated = true;
        InitializeComponent();
        Visibility = Visibility.Collapsed;
        _fadeOut.Completed += FadeOutOnCompleted;
        ModalShown += ModalShow;
    }

    /// <summary>
    /// Gets or sets the <see cref="BlurEffect"/> to be affected when the modal is shown.
    /// </summary>
    public BlurEffect? BlurEffect { get; set; }

    private void ModalShow(string caption, UIElement content, MessageBoxButton button, MessageBoxImage image)
    {
        TitleLabel.Text = caption;
        ContentGrid.Children.Add(content);
        SetButtons(button);
        SetImage(image);
        FadeIn();
    }

    private void SetButtons(MessageBoxButton button)
    {
        ButtonOk.Visibility = Visibility.Visible;
        ButtonCancel.Visibility = button switch
        {
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
        FadeOut();
    }
    
    private void FadeIn()
    {
        Visibility = Visibility.Visible;
        BeginAnimation(OpacityProperty, _fadeIn);
        if (BlurEffect is null) return;
        BlurEffect.Radius = 6;
    }
    
    private void FadeOut()
    {
        BeginAnimation(OpacityProperty, _fadeOut);
        if (BlurEffect is null) return;
        BlurEffect.Radius = 0;
    }
    
    private void FadeOutOnCompleted(object? sender, EventArgs e)
    {
        Visibility = Visibility.Collapsed;
        ContentGrid.Children.Clear();
    }
}