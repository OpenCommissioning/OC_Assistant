using System.IO;
using System.IO.Compression;
using System.Windows;
using EnvDTE;
using Microsoft.Win32;
using OC.Assistant.Core;
using OC.Assistant.Core.TwinCat;
using OC.Assistant.Sdk;

namespace OC.Assistant.Controls;

internal partial class FileMenu : IProjectSelector
{
    private bool _solutionIsOpen;
    private SolutionEvents? _solutionEvents;
    
    private static event Action<DTE>? OnConnectSolution;
    private static event Action? OnOpenSolution;
    private static event Action? OnCreateSolution;
    
    public event Action<DTE>? DteSelected;
    public event Action? DteClosed;
    
    private void SolutionEventsOnAfterClosing()
    {
        if (_solutionEvents is not null)
        {
            _solutionEvents.AfterClosing -= SolutionEventsOnAfterClosing;
            _solutionEvents = null;
        }
        
        DteClosed?.Invoke();
    }
    
    public FileMenu()
    {
        InitializeComponent();
        OnConnectSolution += SelectDte;
        OnOpenSolution += () => OpenSlnOnClick();
        OnCreateSolution += () => CreateSlnOnClick();
        ProjectManager.Instance.Subscribe(this);
    }
    
    public static void ConnectSolution(DTE dte)
    {
        OnConnectSolution?.Invoke(dte);
    }
    
    public static void OpenSolution()
    {
        OnOpenSolution?.Invoke();
    }
    
    public static void CreateSolution()
    {
        OnCreateSolution?.Invoke();
    }

    private async void FileMenuOnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            DteSelector.Selected += SelectDte;
            await InitializeDte();
        }
        catch (Exception exception)
        {
            Logger.LogError(this, exception.Message);
        }
    }
    
    private void ExitOnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private async Task InitializeDte()
    {
        if (!File.Exists(AppData.PreselectedProject)) return;
        var path = await File.ReadAllTextAsync(AppData.PreselectedProject);
        if (!Path.GetExtension(path).Equals(".sln", StringComparison.CurrentCultureIgnoreCase)) return;
        File.Delete(AppData.PreselectedProject);
        
        BusyState.Set(this);
        await GetSolutionFromPath(path);
        BusyState.Reset(this);
    }

    private async Task GetSolutionFromPath(string path)
    {
        await Task.Run(() =>
        {
            var selection = TcDte.GetInstance(path);
            if (selection == null)
            {
                Logger.LogError(this, $"There is no open solution {path}.");
                return;
            }
            SelectDte(selection);
        });
    }
    
    private void SelectDte(DTE dte)
    {
        _solutionEvents = dte.GetSolutionEvents();
        if (_solutionEvents is not null)
        {
            _solutionEvents.AfterClosing += SolutionEventsOnAfterClosing;
        }
        
        Logger.LogInfo(this, dte.GetSolutionFullName() + " connected");
        
        dte.EnableUserControl();
        DteSelected?.Invoke(dte);
    }
    
    private async void OpenSlnOnClick(object? sender = null, RoutedEventArgs? e = null)
    {
        try
        {
            BusyState.Set(this);

            var openFileDialog = new OpenFileDialog
            {
                Filter = "TwinCAT Solution (*.sln)|*.sln",
                RestoreDirectory = true
            };
        
            if (openFileDialog.ShowDialog() == true)
            {
                await OpenDte(openFileDialog.FileName);
            }
        
            BusyState.Reset(this);
        }
        catch (Exception exception)
        {
            Logger.LogError(this, exception.Message);
        }
    }
    
    private async void CreateSlnOnClick(object? sender = null, RoutedEventArgs? e = null)
    {
        try
        {
            BusyState.Set(this);

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "TwinCAT Solution (*.sln)|*.sln",
                RestoreDirectory = true
            };
        
            if (saveFileDialog.ShowDialog() == true)
            {
                await CreateSolution(saveFileDialog.FileName);
            }

            BusyState.Reset(this);
        }
        catch (Exception exception)
        {
            Logger.LogError(this, exception.Message);
        }
    }

    private async Task OpenDte(string path)
    {
        await Task.Run(async () =>
        {
            try
            {
                var dte = TcDte.Create();
                Logger.LogInfo(this, $"Open project '{path}' ...");

                _solutionIsOpen = false;

                _solutionEvents = dte.GetSolutionEvents();
                if (_solutionEvents is not null)
                {
                    _solutionEvents.Opened += SolutionEventsOnOpened;
                }
                
                dte.OpenSolution(path);
                while (!_solutionIsOpen) await Task.Delay(100);
                SelectDte(dte);
            }
            catch (Exception e)
            {
                Logger.LogError(this, e.Message);
            }
        });
    }
    
    private void SolutionEventsOnOpened()
    {
        _solutionIsOpen = true;
        if (_solutionEvents is null) return;
        _solutionEvents.Opened -= SolutionEventsOnOpened;
    }

    private async Task CreateSolution(string slnFilePath)
    {
        const string templateName = "OC.TcTemplate";
        var rootFolder = Path.GetDirectoryName(slnFilePath);
        var projectName = Path.GetFileNameWithoutExtension(slnFilePath);

        try
        {
            if (rootFolder is null)
            {
                throw new ArgumentNullException(rootFolder);
            }
            
            //Get zip file from resource
            var assembly = typeof(FileMenu).Assembly;
            var resourceName = $"{assembly.GetName().Name}.Resources.{templateName}.zip";
            var resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream is null)
            {
                throw new ArgumentNullException(resourceName);
            }
            
            //Extract resource to folder
            ZipFile.ExtractToDirectory(resourceStream, rootFolder);

            //Rename solution file
            File.Move($"{rootFolder}\\{templateName}.sln", slnFilePath);
                
            //Modify solution file
            var slnFileText = await File.ReadAllTextAsync(slnFilePath);
            await File.WriteAllTextAsync(slnFilePath, slnFileText.Replace(templateName, projectName));
                
            //Rename project folder
            Directory.Move($"{rootFolder}\\{templateName}", $"{rootFolder}\\{projectName}");
                
            //Rename project file
            File.Move($@"{rootFolder}\{projectName}\{templateName}.tsproj", $@"{rootFolder}\{projectName}\{projectName}.tsproj");
        }
        catch(Exception e)
        {
            Logger.LogError(this, e.Message);
            return;
        }

        await OpenDte(slnFilePath);
    }
}