using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using OC.Assistant.Models;
using OC.Assistant.Services;
using OC.Assistant.ViewModels;
using OC.Assistant.Views;

namespace OC.Assistant;

public class App : Application
{
    public static AppSettings Settings { get; } = new AppSettings().Read();

    public static void InvokeOnMainThread(Action callback)
        => Avalonia.Threading.Dispatcher.UIThread.Invoke(callback);

    //public override void Initialize()
    //{
    //    AvaloniaXamlLoader.Load(this);
    //}

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            services.AddSingleton<LoggingService>();
            services.AddSingleton<AppService>();
            services.AddSingleton<WebService>();
            services.AddSingleton<LegacyApiService>();
            services.AddSingleton<PluginService>();
            services.AddTransient<MainViewModel>();

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<LoggingService>();
            serviceProvider.GetRequiredService<WebService>();
            serviceProvider.GetRequiredService<LegacyApiService>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
