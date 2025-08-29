using System.IO;
using System.IO.Compression;
using System.Windows;
using EnvDTE;
using Microsoft.Win32;
using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.Twincat;

public partial class WelcomePage
{
    public WelcomePage()
        => InitializeComponent();
    
    private void OpenOnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "TwinCAT Solution (*.sln)|*.sln",
            RestoreDirectory = true
        };
        
        if (openFileDialog.ShowDialog() == true)
        {
            OpenSolution(() => openFileDialog.FileName);
        }
    }
    
    private void CreateOnClick(object? sender = null, RoutedEventArgs? e = null)
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

    private void OpenSolution(Func<string?> getSolutionFullName)
    {
        Task.Run(() =>
        {
            string? solutionFullName = null;
            string? projectFolder = null;
            
            DteSingleThread.Run(() =>
            {
                solutionFullName = getSolutionFullName.Invoke();
                if (string.IsNullOrEmpty(solutionFullName)) return;
                
                DTE? dte = null;
                Solution? solution = null;

                try
                {
                    dte = TcDte.Create();
                    solution = dte.Solution;
                    Logger.LogInfo(this, $"Open project '{solutionFullName}' ...");
                    solution?.Open(solutionFullName);
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
                    ComHelper.ReleaseObject(solution);
                    ComHelper.ReleaseObject(dte);
                }
            }).Join();
            
            if (string.IsNullOrEmpty(solutionFullName) || string.IsNullOrEmpty(projectFolder))
            {
                Logger.LogError(this, "Failed to connect solution");
                return;
            }
            ProjectState.Control.Connect(solutionFullName, projectFolder);
        });
    }
    
    private void CreateSolution(string slnFilePath)
    {
        OpenSolution(() =>
        {
            try
            {
                const string templateName = "OC.TcTemplate";
                var rootFolder = Path.GetDirectoryName(slnFilePath);
                var projectName = Path.GetFileNameWithoutExtension(slnFilePath);
                
                if (rootFolder is null)
                {
                    throw new ArgumentNullException(rootFolder);
                }

                //Get zip file from resource
                var assembly = typeof(WelcomePage).Assembly;
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
                
                return slnFilePath;
            }
            catch (Exception e)
            {
                Logger.LogError(this, e.Message);
                return null;
            }
        });
    }
}