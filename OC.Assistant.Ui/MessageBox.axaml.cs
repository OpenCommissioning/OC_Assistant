using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;

namespace OC.Assistant.Ui;

/// <summary>
/// MessageBox buttons.
/// </summary>
public enum MessageBoxButton
{
    /// <summary>Displays an OK button.</summary>
    Ok,
    /// <summary>Displays OK and Cancel buttons.</summary>
    OkCancel
}

/// <summary>
/// MessageBox images.
/// </summary>
public enum MessageBoxImage
{
    /// <summary>No icon.</summary>
    None,
    /// <summary>Information icon.</summary>
    Information,
    /// <summary>Question icon.</summary>
    Question,
    /// <summary>Warning icon.</summary>
    Warning,
    /// <summary>Error icon.</summary>
    Error
}

/// <summary>
/// Represents a custom-themed message box dialog.
/// </summary>
public partial class MessageBox : Window
{
    private bool _result;
    private static readonly WindowIcon NoIcon = new(new WriteableBitmap(
        new PixelSize(32, 32), 
        new Vector(96, 96), 
        PixelFormat.Bgra8888, 
        AlphaFormat.Premul));
    
    /// <summary>
    /// Condition for the result when closing the message box.
    /// </summary>
    public Func<bool>? Condition { get; init; }
    
    /// <summary>
    /// Asynchronous condition for the result when closing the message box.
    /// </summary>
    public Func<Task<bool>>? AsyncCondition { get; init; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBox"/> class.
    /// </summary>
    public MessageBox()
    {
        InitializeComponent();
        Icon = NoIcon;
    }

    /// <summary>
    /// Shows the message box with the given string content.
    /// </summary>
    public static Task<bool> Show(
        string content,
        MessageBoxButton button,
        MessageBoxImage image) =>
        Show(new Label { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Content = content }, button, image);

    /// <summary>
    /// Shows the message box with the given string content and a synchronous close condition.
    /// </summary>
    public static Task<bool> Show(
        string content,
        MessageBoxButton button,
        MessageBoxImage image,
        Func<bool> condition) =>
        Show(new Label { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Content = content }, button, image, condition);

    /// <summary>
    /// Shows the message box with the given string content and an asynchronous close condition.
    /// </summary>
    public static Task<bool> Show(
        string content,
        MessageBoxButton button,
        MessageBoxImage image,
        Func<Task<bool>> condition) =>
        Show(new Label { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Content = content }, button, image, condition);

    /// <summary>
    /// Shows the message box with custom control content.
    /// </summary>
    public static async Task<bool> Show(
        Control content,
        MessageBoxButton button,
        MessageBoxImage image)
    {
        var box = Build(content, button, image);
        return await ShowDialog(box);
    }

    /// <summary>
    /// Shows the message box with custom control content and a synchronous close condition.
    /// </summary>
    public static async Task<bool> Show(
        Control content,
        MessageBoxButton button,
        MessageBoxImage image,
        Func<bool> condition)
    {
        var box = Build(content, button, image, condition);
        return await ShowDialog(box);
    }

    /// <summary>
    /// Shows the message box with custom control content and an asynchronous close condition.
    /// </summary>
    public static async Task<bool> Show(
        Control content,
        MessageBoxButton button,
        MessageBoxImage image,
        Func<Task<bool>> condition)
    {
        var box = Build(content, button, image, asyncCondition: condition);
        return await ShowDialog(box);
    }

    private static MessageBox Build(
        Control content,
        MessageBoxButton button,
        MessageBoxImage image,
        Func<bool>? condition = null,
        Func<Task<bool>>? asyncCondition = null)
    {
        var box = new MessageBox
        {
            Condition = condition,
            AsyncCondition = asyncCondition
        };
        
        box.ContentGrid.Children.Add(content);
        SetButtons(box, button);
        SetImage(box, image);
        return box;
    }

    private static async Task<bool> ShowDialog(MessageBox box)
    {
        var mainWindow = 
            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.MainWindow;
        
        if (mainWindow is not null) return await box.ShowDialog<bool>(mainWindow);

        box.Show();
        return await box.GetDialogResultAsync();
    }

    private Task<bool> GetDialogResultAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        Closed += (_, _) => tcs.TrySetResult(_result);
        return tcs.Task;
    }

    private static void SetButtons(MessageBox box, MessageBoxButton button)
    {
        if (button == MessageBoxButton.Ok)
            box.ButtonCancel.IsVisible = false;
    }

    private static void SetImage(MessageBox box, MessageBoxImage image)
    {
        if (image == MessageBoxImage.None)
        {
            box.IconControl.IsVisible = false;
            return;
        }

        var key = image switch
        {
            MessageBoxImage.Information => "InformationIcon",
            MessageBoxImage.Question => "QuestionIcon",
            MessageBoxImage.Warning => "WarningIcon",
            MessageBoxImage.Error => "ErrorIcon",
            _ => null
        };

        if (key is null) return;

        var theme = Application.Current?.ActualThemeVariant;
        if (Application.Current?.Styles.TryGetResource(key, theme, out var resource) == true &&
            resource is IControlTemplate template)
        {
            box.IconControl.Template = template;
        }
    }

    private async void ButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var isOk = ReferenceEquals(sender, ButtonOk);

        if (isOk)
        {
            if (Condition is not null && !Condition())
            {
                await Shake();
                return;
            }

            if (AsyncCondition is not null)
            {
                BusyOverlay.IsVisible = true;
                var passed = await AsyncCondition();
                BusyOverlay.IsVisible = false;
                if (!passed)
                {
                    await Shake();
                    return;
                }
            }
        }

        _result = isOk;
        Close(isOk);
    }
    
    private void WindowOnClosed(object? sender, EventArgs e)
    {
        Close(_result);
    }

    private async Task Shake()
    {
        var translate = new TranslateTransform();
        RenderTransform = translate;
        int[] offsets = [8, -8, 6, -6, 4, -4, 0];
        foreach (var offset in offsets)
        {
            translate.X = offset;
            await Task.Delay(40);
        }
        RenderTransform = null;
    }
}
