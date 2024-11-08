using System.Windows;
using System.Windows.Input;

namespace OC.Assistant.Theme;


/// <summary>
/// Represents a custom message box with an appropriate theme style.
/// </summary>
public partial class MessageBox
{
    private static MessageBox? _messageBox;
    private static MessageBoxResult _result = MessageBoxResult.No;
    
    private MessageBox()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// Shows the message box with the given parameters. 
    /// </summary>
    /// <param name="caption">The caption of the message box.</param>
    /// <param name="text">The text within the message box.</param>
    /// <param name="button">The <see cref="MessageBoxButton"/> of the message box.</param>
    /// <param name="image">The shown image on the left side.</param>
    /// <returns></returns>
    public static MessageBoxResult Show(string caption, string text, MessageBoxButton button, MessageBoxImage image)
    {
        _messageBox = new MessageBox
        {
            Text = { Text = text }, 
            TitleLabel = { Text = caption },
            Title = caption
        };
        SetVisibilityOfButtons(_messageBox, button);
        SetImageOfMessageBox(_messageBox, image);
        _messageBox.ShowDialog();
        return _result;
    }
    
    
    /// <summary>
    /// Shows the message box with the given parameters. 
    /// </summary>
    /// <param name="caption">The caption of the message box.</param>
    /// <param name="content">The content within the message box.</param>
    /// <param name="button">The <see cref="MessageBoxButton"/> of the message box.</param>
    /// <param name="image">The shown image on the left side.</param>
    /// <returns></returns>
    public static MessageBoxResult Show(string caption, UIElement content, MessageBoxButton button, MessageBoxImage image)
    {
        _messageBox = new MessageBox
        {
            TitleLabel = { Text = caption },
            Title = caption
        };
        _messageBox.ContentGrid.Children.Add(content);
        SetVisibilityOfButtons(_messageBox, button);
        SetImageOfMessageBox(_messageBox, image);
        _messageBox.ShowDialog();
        return _result;
    }

    private static void SetVisibilityOfButtons(MessageBox messageBox, MessageBoxButton button)
    {
        switch (button)
        {
            case MessageBoxButton.OK:
                messageBox.ButtonCancel.Visibility = Visibility.Collapsed;
                messageBox.ButtonNo.Visibility = Visibility.Collapsed;
                messageBox.ButtonYes.Visibility = Visibility.Collapsed;
                break;
            case MessageBoxButton.OKCancel:
                messageBox.ButtonNo.Visibility = Visibility.Collapsed;
                messageBox.ButtonYes.Visibility = Visibility.Collapsed;
                break;
            case MessageBoxButton.YesNo:
                messageBox.ButtonOk.Visibility = Visibility.Collapsed;
                messageBox.ButtonCancel.Visibility = Visibility.Collapsed;
                break;
            case MessageBoxButton.YesNoCancel:
                messageBox.ButtonOk.Visibility = Visibility.Collapsed;
                break;
            default:
                messageBox.ButtonCancel.Visibility = Visibility.Collapsed;
                messageBox.ButtonNo.Visibility = Visibility.Collapsed;
                messageBox.ButtonYes.Visibility = Visibility.Collapsed;
                break;
        }
    }

    private static void SetImageOfMessageBox(MessageBox messageBox, MessageBoxImage image)
    {
        messageBox.SymbolInformation.Visibility =
            image == MessageBoxImage.Information ? Visibility.Visible : Visibility.Collapsed;
        
        messageBox.SymbolWarning.Visibility =
            image == MessageBoxImage.Warning ? Visibility.Visible : Visibility.Collapsed;
        
        messageBox.SymbolError.Visibility =
            image == MessageBoxImage.Error ? Visibility.Visible : Visibility.Collapsed;
        
        messageBox.SymbolQuestion.Visibility =
            image == MessageBoxImage.Question ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender.Equals(ButtonOk)) _result = MessageBoxResult.OK;
        else if (sender.Equals(ButtonYes)) _result = MessageBoxResult.Yes;
        else if (sender.Equals(ButtonNo)) _result = MessageBoxResult.No;
        else if (sender.Equals(ButtonCancel)) _result = MessageBoxResult.Cancel;
        else _result = MessageBoxResult.None;
        
        _messageBox?.Close();
        _messageBox = null;
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}