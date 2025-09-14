using Microsoft.Extensions.Logging;

namespace OC.Assistant.Core;

public class WebApiLogger : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new Logger(categoryName);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private class Logger(string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var message = $"{category}: {formatter(state, exception)}";
            if (exception is not null) message += Environment.NewLine + exception;

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                    Sdk.Logger.LogInfo(typeof(WebApiLogger), message, true);
                    break;
                case LogLevel.Warning:
                    Sdk.Logger.LogWarning(typeof(WebApiLogger), message, true);
                    break;
                case LogLevel.Critical:
                case LogLevel.Error:
                    Sdk.Logger.LogError(typeof(WebApiLogger), message, true);
                    break;
                case LogLevel.None:
                default: 
                    break;
            }
        }
    }
}