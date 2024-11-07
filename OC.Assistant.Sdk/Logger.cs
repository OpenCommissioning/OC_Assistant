namespace OC.Assistant.Sdk;

/// <summary>
/// Static class to log messages to the <see cref="OC.Assistant"/>.
/// </summary>
public static class Logger
{
    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="sender">The sender from where the message is sent.</param>
    /// <param name="message">The message itself.</param>
    /// <param name="verbose">Only send the message if verbose logging is enabled.</param>
    public static void LogInfo(object sender, string message, bool verbose = false)
    {
        if (!Verbose && verbose) return;
        Info?.Invoke(sender, message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="sender">The sender from where the message is sent.</param>
    /// <param name="message">The message itself.</param>
    /// <param name="verbose">Only send the message if verbose logging is enabled.</param>
    public static void LogWarning(object sender, string message, bool verbose = false)
    {
        if (!Verbose && verbose) return;
        Warning?.Invoke(sender, message);
    }
    
    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="sender">The sender from where the message is sent.</param>
    /// <param name="message">The message itself.</param>
    /// <param name="verbose">Only send the message if verbose logging is enabled.</param>
    public static void LogError(object sender, string message, bool verbose = false)
    {
        if (!Verbose && verbose) return;
        Error?.Invoke(sender, message);
    }
    
    /// <summary>Represents the method that will handle a logging event.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="message">The message.</param>
    public delegate void LogHandler(object sender, string message);
    
    /// <summary>
    /// An info has been logged.
    /// </summary>
    public static event LogHandler? Info;
    
    /// <summary>
    /// A warning has been logged.
    /// </summary>
    public static event LogHandler? Warning;
    
    /// <summary>
    /// An error has been logged.
    /// </summary>
    public static event LogHandler? Error;
    
    /// <summary>
    /// Enables/disables verbose messages.
    /// </summary>
    public static bool Verbose { get; set; }
}