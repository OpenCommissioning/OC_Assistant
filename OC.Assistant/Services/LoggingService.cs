using OC.Assistant.Sdk;
using OC.Assistant.Ui;
using Serilog;

namespace OC.Assistant.Services;

public class LoggingService
{
    public LoggingService()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(
                path: Path.Combine(AppData.Path, "log.txt"),
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 10_240_000, // ~10 MB
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 10,
                flushToDiskInterval: TimeSpan.FromSeconds(1)
            )
            .CreateLogger();
        
        Logger.Info += LogInfo;
        Logger.Warning += LogWarning;
        Logger.Error += LogError;
    }

    private static void LogInfo(object sender, string message)
    {
        Log.Information("[{Sender}] {Message}", sender, message);
        LogViewer.LogInfo(sender, message);
    }
    
    private static void LogWarning(object sender, string message)
    {
        Log.Warning("[{Sender}] {Message}", sender, message);
        LogViewer.LogWarning(sender, message);
    }
    
    private static void LogError(object sender, string message)
    {
        Log.Error("[{Sender}] {Message}", sender, message);
        LogViewer.LogError(sender, message);
    }
        
}