using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using OC.Assistant.Theme.Internals;

namespace OC.Assistant.Theme;

/// <summary>
/// Represents the message type.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// The message shows an info.
    /// </summary>
    Info,
    /// <summary>
    /// The message shows a warning.
    /// </summary>
    Warning,
    /// <summary>
    /// The message shows an error.
    /// </summary>
    Error
}

/// <summary>
/// Represents a message.
/// </summary>
/// <param name="sender">The sender of the message.</param>
/// <param name="message">The message text.</param>
/// <param name="type">The message type.</param>
internal class Message(object sender, string message, MessageType type)
{
    /// <summary>
    /// Gets a <see cref="DateTime"/> value that represents the time of creation of this instance.
    /// </summary>
    private DateTime DateTime { get; } = DateTime.Now;

    /// <summary>
    /// The type of this message.
    /// </summary>
    public MessageType Type => type;
    
    /// <summary>
    /// The header of the message containing timestamp and sender information.
    /// </summary>
    public string Header => $"{DateTime:yyyy-MM-dd HH:mm:ss.fff} | {sender.ToString()?.Split(':')[0]}";
    
    /// <summary>
    /// The message text.
    /// </summary>
    public string Text => message;
    
    /// <summary>
    /// The icon depending on the <see cref="MessageType"/>.
    /// </summary>
    public string Icon => Type switch
    {
        MessageType.Info => "\xE946",
        MessageType.Warning => "\xE7BA",
        MessageType.Error => "\xEA39",
        _ => "\xE946"
    };
    
    /// <summary>
    /// The icon color depending on the <see cref="MessageType"/>.
    /// </summary>
    public Brush? IconColor => Type switch
    {
        MessageType.Info => Application.Current.Resources["InfoBrush"] as Brush,
        MessageType.Warning => Application.Current.Resources["WarningBrush"] as Brush,
        MessageType.Error => Application.Current.Resources["DangerBrush"] as Brush,
        _ => Application.Current.Resources["InfoBrush"] as Brush
    };

    /// <summary>
    /// The concatenated string for the logFile.
    /// </summary>
    public string LogFileText => $"{Type} | {Header} | {Text}\n";
}

/// <summary>
/// Represents a <see cref="UserControl"/> to display log messages.
/// </summary>
public partial class LogViewer
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Queue<Message> _messageQueue = [];
    private readonly object _queueLock = new ();
    private readonly ObservableRangeExtension<Message> _messageBuffer =[];
    private readonly ICollectionView _collectionView;
    private bool _infoFilterEnabled = true;
    private bool _warningFilterEnabled = true;
    private bool _errorFilterEnabled = true;
    private ScrollViewer? _scrollViewer;

    /// <summary>
    /// Gets or sets the path of the logFile. When the path is <c>null</c>, no logFile is created.
    /// </summary>
    public string? LogFilePath { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="LogViewer"/>.
    /// </summary>
    public LogViewer()
    {
        InitializeComponent();
        
        ItemsControl.ItemsSource = _messageBuffer;
        _messageBuffer.CollectionChanged += (_, _) => _scrollViewer?.ScrollToBottom();
        _collectionView = CollectionViewSource.GetDefaultView(ItemsControl.ItemsSource);
        _collectionView.Filter = UserFilter;
    }

    /// <summary>
    /// Adds a new message to the <see cref="LogViewer"/>.
    /// Also logs the message to a logFile, of <see cref="LogFilePath"/> is set.
    /// </summary>
    /// <param name="sender">The sender of the message.</param>
    /// <param name="message">The message text.</param>
    /// <param name="type">The message type.</param>
    public void Add(object sender, string message, MessageType type)
    {
        lock (_queueLock)
        {
            _messageQueue.Enqueue(new Message(sender, message, type));
        }
    }

    private void Console_OnLoaded(object sender, RoutedEventArgs e)
    {
        _scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(ItemsControl, 0);
        Task.Run(LogCycle);
    }

    private bool UserFilter(object item)
    {
        var type = ((Message)item).Type;
        return (type is MessageType.Info && _infoFilterEnabled) ||
               (type is MessageType.Warning && _warningFilterEnabled) ||
               (type is MessageType.Error && _errorFilterEnabled);
    }
    
    private async Task LogCycle()
    {
        var token = _cancellationTokenSource.Token;
        var stopwatch = new Stopwatch();

        while (!token.IsCancellationRequested)
        {
            var delta = 100 - (int) stopwatch.Elapsed.TotalMilliseconds;
            if (delta > 0) Thread.Sleep(delta);
            stopwatch.Restart();

            Message[] messages;
            
            lock (_queueLock)
            {
                if (_messageQueue.Count == 0) continue;
                messages = _messageQueue.ToArray();
                _messageQueue.Clear();
            }
            
            Dispatcher.Invoke(() =>
            {
                _messageBuffer.AddRange(messages);
                if (_messageBuffer.Count > 10000)
                {
                    _messageBuffer.RemoveRange(0, _messageBuffer.Count - 8000);
                }
            });
            
            await WriteToLogFile(messages.Aggregate("", (current, message) => current + message.LogFileText));
        }
    }
    
    private async Task WriteToLogFile(string logFileText)
    {
        if (LogFilePath is null) return;
        if (logFileText == "") return;
        var streamWriter = new StreamWriter(LogFilePath, true, Encoding.UTF8);
        await streamWriter.WriteAsync(logFileText);
        streamWriter.Dispose();
        
        if (new FileInfo(LogFilePath).Length < 10240000L) return;
        File.Copy(LogFilePath, 
            $"{LogFilePath.Replace(".log", "")}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
        File.WriteAllText(LogFilePath, "");
    }

    private void ClearOnClick(object sender, RoutedEventArgs e)
    {
        _messageBuffer.Clear();
    }
    
    private void ErrorFilterOnClick(object sender, RoutedEventArgs e)
    {
        _errorFilterEnabled = ((ToggleButton) sender).IsChecked == true;
        _collectionView.Refresh();
    }

    private void WarningFilterOnClick(object sender, RoutedEventArgs e)
    {
        _warningFilterEnabled = ((ToggleButton) sender).IsChecked == true;
        _collectionView.Refresh();
    }
        
    private void InfoFilterOnClick(object sender, RoutedEventArgs e)
    {
        _infoFilterEnabled = ((ToggleButton) sender).IsChecked == true;
        _collectionView.Refresh();
    }
}