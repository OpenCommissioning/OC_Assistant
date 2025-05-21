using System.IO;
using System.IO.Compression;
using System.Windows;
using EnvDTE;
using Microsoft.Win32;
using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.Controls;

internal partial class FileMenu
{
    private static event RoutedEventHandler? OnOpenSolution;
    private static event RoutedEventHandler? OnCreateSolution;
    
    public FileMenu()
    {
        InitializeComponent();
        OnOpenSolution += OpenSlnOnClick;
        OnCreateSolution += CreateSlnOnClick;
    }
    
    public static void OpenSolution(object sender, RoutedEventArgs e)
    {
        OnOpenSolution?.Invoke(sender, e);
    }
    
    public static void CreateSolution(object sender, RoutedEventArgs e)
    {
        OnCreateSolution?.Invoke(sender, e);
    }
    
    private void ExitOnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
    
    private void OpenSlnOnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "TwinCAT Solution (*.sln)|*.sln",
            RestoreDirectory = true
        };
        
        if (openFileDialog.ShowDialog() == true)
        {
            OpenDte(openFileDialog.FileName);
        }
    }
    
    private void CreateSlnOnClick(object? sender = null, RoutedEventArgs? e = null)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "TwinCAT Solution (*.sln)|*.sln",
            RestoreDirectory = true
        };
        
        if (saveFileDialog.ShowDialog() == true)
        {
            CreateSolution(saveFileDialog.FileName);
        }
    }

    private void OpenDte(string path, Task? previousTask = null)
    {
        string? projectFolder = null;
        
        var thread = DteSingleThread.Run(() =>
        {
            previousTask?.Wait();
            if (previousTask?.IsFaulted == true) return; 

            DTE? dte = null;

            try
            {
                dte = TcDte.Create();
                Logger.LogInfo(this, $"Open project '{path}' ...");
                dte.Solution?.Open(path);
                dte.UserControl = true;
                if (!dte.UserControl) return;
                projectFolder = dte.GetProjectFolder();
            }
            catch (Exception e)
            {
                Logger.LogError(this, e.Message);
            }
            finally
            {
                dte?.Finalize();
            }
        });
        
        Task.Run(() =>
        {
            thread.Join();
            if (projectFolder is null)
            {
                Logger.LogError(this, "Failed to connect solution");
                return;
            }
            ProjectState.Solution.Connect(path, projectFolder);
        });
    }
    
    private void CreateSolution(string slnFilePath)
    {
        var task = Task.Run(() =>
        {
            try
            {
                BusyState.Set(this);
                
                const string templateName = "OC.TcTemplate";
                var rootFolder = Path.GetDirectoryName(slnFilePath);
                var projectName = Path.GetFileNameWithoutExtension(slnFilePath);
                
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
                var slnFileText = File.ReadAllText(slnFilePath);
                File.WriteAllText(slnFilePath, slnFileText.Replace(templateName, projectName));

                //Rename project folder
                Directory.Move($"{rootFolder}\\{templateName}", $"{rootFolder}\\{projectName}");

                //Rename project file
                File.Move($@"{rootFolder}\{projectName}\{templateName}.tsproj",
                    $@"{rootFolder}\{projectName}\{projectName}.tsproj");
                
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Logger.LogError(this, e.Message);
                return Task.FromException(e);
            }
            finally
            {
                BusyState.Reset(this);
            }
        });
        
        OpenDte(slnFilePath, task);
    }
}