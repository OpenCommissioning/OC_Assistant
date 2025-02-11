using System.IO;
using System.IO.Compression;
using System.Windows;
using Microsoft.Win32;
using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.Controls;

internal partial class FileMenu
{
    private static event Action? OnOpenSolution;
    private static event Action? OnCreateSolution;
    
    public FileMenu()
    {
        InitializeComponent();
        OnOpenSolution += () => OpenSlnOnClick();
        OnCreateSolution += () => CreateSlnOnClick();
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
            await Task.Delay(10);
            if (!File.Exists(AppData.PreselectedProject)) return;
            var path = await File.ReadAllTextAsync(AppData.PreselectedProject);
            File.Delete(AppData.PreselectedProject);
            ProjectState.Solution.Connect(path);
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
                
                dte.OpenSolution(path);
                while (!dte.GetSolutionIsOpen()) await Task.Delay(100);
                dte.EnableUserControl();
                ProjectState.Solution.Connect(path);
                dte.Finalize();
            }
            catch (Exception e)
            {
                Logger.LogError(this, e.Message);
            }
        });
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