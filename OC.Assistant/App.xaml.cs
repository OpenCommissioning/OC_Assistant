using System.Windows;

namespace OC.Assistant;

public partial class App
{
    private static Mutex? _mutex;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        const string mutexName = "OC.Assistant";
        
        _mutex = new Mutex(true, mutexName, out var createdNew);

        if (!createdNew)
        {
            MessageBox.Show($"{mutexName} is already running.");
            Current.Shutdown();
            return;
        }

        base.OnStartup(e);
    }
}