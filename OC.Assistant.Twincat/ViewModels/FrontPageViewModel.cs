using System.IO.Compression;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using EnvDTE;
using OC.Assistant.Sdk;

namespace OC.Assistant.Twincat.ViewModels;

public partial class FrontPageViewModel
{
    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow?.StorageProvider;
        return null;
    }

    [RelayCommand]
    private async Task Open()
    {
        var provider = GetStorageProvider();
        if (provider is null) return;

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("TwinCAT Solution") { Patterns = ["*.sln"] }]
        });

        if (files.Count == 1)
            OpenSolution(() => files[0].Path.LocalPath);
    }

    [RelayCommand]
    private async Task Create()
    {
        var provider = GetStorageProvider();
        if (provider is null) return;

        var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            DefaultExtension = ".sln",
            SuggestedFileName = "TcProject.sln",
            FileTypeChoices = [new FilePickerFileType("TwinCAT Solution") { Patterns = ["*.sln"] }]
        });

        if (file is not null)
            CreateSolution(file.Path.LocalPath);
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
                    TcDte.ReleaseObject(solution);
                    TcDte.ReleaseObject(dte);
                }
            }).Join();

            if (string.IsNullOrEmpty(solutionFullName) || string.IsNullOrEmpty(projectFolder))
            {
                Logger.LogError(this, "Failed to connect solution");
                return;
            }

            var projectFile = Path.Combine(projectFolder, "OC.Assistant.xml");
            TcState.Singleton.ConnectSolution(projectFile, solutionFullName);
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

                var assembly = typeof(FrontPageViewModel).Assembly;
                var resourceName = $"{assembly.GetName().Name}.Resources.{templateName}.zip";
                var resourceStream = assembly.GetManifestResourceStream(resourceName);
                if (resourceStream is null)
                {
                    throw new ArgumentNullException(resourceName);
                }

                ZipFile.ExtractToDirectory(resourceStream, rootFolder);

                File.Move(Path.Combine(rootFolder, $"{templateName}.sln"), slnFilePath);

                var slnFileText = File.ReadAllText(slnFilePath);
                File.WriteAllText(slnFilePath, slnFileText.Replace(templateName, projectName));

                Directory.Move(Path.Combine(rootFolder, templateName), Path.Combine(rootFolder, projectName));
                
                File.Move(Path.Combine(rootFolder, projectName, $"{templateName}.tsproj"),
                    Path.Combine(rootFolder, projectName, $"{projectName}.tsproj"));

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
