using System.Collections.Concurrent;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace OC.Assistant.Ui;

/// <summary>
/// Represents the message type.
/// </summary>
internal enum MessageType
{
    Info,
    Warning,
    Error
}

/// <summary>
/// Represents a log message.
/// </summary>
internal class Message
{
    public Message(object sender, string message, MessageType type)
    {
        Sender = sender;
        Text = message;
        Type = type;
        DateTime = DateTime.Now;
        Header = $"{DateTime:yyyy-MM-dd HH:mm:ss.fff}";
    }

    public DateTime DateTime { get; }
    public MessageType Type { get; }
    public string Header { get; }
    public string Text { get; }
    public object Sender { get; }

    public bool IsInfo => Type == MessageType.Info;
    public bool IsWarning => Type == MessageType.Warning;
    public bool IsError => Type == MessageType.Error;
}

/// <summary>
/// Represents a <see cref="UserControl"/> to display log messages.
/// </summary>
public partial class LogViewer : Border
{
    private const int MAX_BUFFER_SIZE = 10000;
    private const int TRIM_TARGET_SIZE = 8000;
    private const int MAX_DRAIN_PER_TICK = 1000;

    private static readonly ConcurrentQueue<Message> MessageQueue = [];
    private readonly List<Message> _messageBuffer = [];
    private readonly AvaloniaList<Message> _filteredMessages = [];
    private DispatcherTimer? _timer;
    private bool _infoFilterEnabled = true;
    private bool _warningFilterEnabled = true;
    private bool _errorFilterEnabled = true;
    private bool _clearRequested;

    /// <summary>Logs an info message.</summary>
    public static void LogInfo(object sender, string message) =>
        MessageQueue.Enqueue(new Message(sender, message, MessageType.Info));

    /// <summary>Logs a warning message.</summary>
    public static void LogWarning(object sender, string message) =>
        MessageQueue.Enqueue(new Message(sender, message, MessageType.Warning));

    /// <summary>Logs an error message.</summary>
    public static void LogError(object sender, string message) =>
        MessageQueue.Enqueue(new Message(sender, message, MessageType.Error));

    /// <summary>
    /// Creates a new instance of <see cref="LogViewer"/>.
    /// </summary>
    public LogViewer()
    {
        InitializeComponent();
        ItemsControl.ItemsSource = _filteredMessages;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_timer is not null) return;
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private bool UserFilter(Message item) =>
        (item.Type is MessageType.Info && _infoFilterEnabled) ||
        (item.Type is MessageType.Warning && _warningFilterEnabled) ||
        (item.Type is MessageType.Error && _errorFilterEnabled);

    private void OnTick(object? sender, EventArgs e)
    {
        if (_clearRequested)
        {
            _clearRequested = false;
            _messageBuffer.Clear();
            _filteredMessages.Clear();
        }

        if (MessageQueue.IsEmpty) return;

        var addedFiltered = new List<Message>();
        var drained = 0;
        while (drained < MAX_DRAIN_PER_TICK && MessageQueue.TryDequeue(out var item))
        {
            _messageBuffer.Add(item);
            if (UserFilter(item)) addedFiltered.Add(item);
            drained++;
        }

        if (_messageBuffer.Count > MAX_BUFFER_SIZE)
        {
            _messageBuffer.RemoveRange(0, _messageBuffer.Count - TRIM_TARGET_SIZE);
            // Rebuild rather than RemoveRange from the head — VirtualizingStackPanel
            // throws "Invalid Arrange rectangle" on combined head-remove + tail-add.
            _filteredMessages.Clear();
            _filteredMessages.AddRange(_messageBuffer.Where(UserFilter));
            ScrollToBottom();
            return;
        }

        if (addedFiltered.Count == 0) return;
        _filteredMessages.AddRange(addedFiltered);
        ScrollToBottom();
    }

    private void RefreshFiltered()
    {
        _filteredMessages.Clear();
        _filteredMessages.AddRange(_messageBuffer.Where(UserFilter));
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        if (_filteredMessages.Count > 0)
            ItemsControl.ScrollIntoView(_filteredMessages.Count - 1);
    }

    private async void ClearOnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            _clearRequested = await MessageBox.Show(
                "Clear all messages?",
                MessageBoxButton.OkCancel,
                MessageBoxImage.Question);
        }
        catch (Exception ex)
        {
            LogError(this, ex.Message);
        }
    }

    private void InfoFilterOnClick(object? sender, RoutedEventArgs e)
    {
        _infoFilterEnabled = ((ToggleButton)sender!).IsChecked == true;
        RefreshFiltered();
    }

    private void WarningFilterOnClick(object? sender, RoutedEventArgs e)
    {
        _warningFilterEnabled = ((ToggleButton)sender!).IsChecked == true;
        RefreshFiltered();
    }

    private void ErrorFilterOnClick(object? sender, RoutedEventArgs e)
    {
        _errorFilterEnabled = ((ToggleButton)sender!).IsChecked == true;
        RefreshFiltered();
    }

    private void ItemsControlOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        DetailBox.Clear();
        
        if (ItemsControl.SelectedItem is not Message selectedMessage) return;
        
        if (MainGrid.RowDefinitions[2].Height.Value == 0)
        {
            MainGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            MainGrid.RowDefinitions[2].Height = new GridLength(4);
            MainGrid.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
            MainGrid.RowDefinitions[3].MinHeight = 24;
            GridSplitter.IsVisible = true;
            Detail.IsVisible = true;
        }
            
        DetailBox.Text = $"""
                          Type: {selectedMessage.Type}
                          DateTime: {selectedMessage.DateTime:yyyy-MM-dd HH:mm:ss.fff}
                          Sender: {selectedMessage.Sender}
                          Message: {selectedMessage.Text}
                          """;
    }

    private void CloseDetailOnClick(object? sender, RoutedEventArgs e)
    {
        ItemsControl.SelectedItem = null;
        MainGrid.RowDefinitions[2].Height = new GridLength(0);
        MainGrid.RowDefinitions[3].Height = new GridLength(0);
        MainGrid.RowDefinitions[3].MinHeight = 0;
        GridSplitter.IsVisible = false;
        Detail.IsVisible = false;
    }
}
