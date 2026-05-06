using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Sdk;
using OC.Assistant.Ui;
using OC.Assistant.Twincat.Automation;
using TwinCAT.Ads;

namespace OC.Assistant.Twincat.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        TcState.Singleton.AdsStateChanged += OnAdsStateChanged;
    }

    [ObservableProperty]
    public partial bool RunIndicatorVisibility { get; set; }

    [ObservableProperty]
    public partial bool ConfigIndicatorVisibility { get; set; }

    [ObservableProperty]
    public partial string? NedId { get; set; }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsContentEnabled { get; set; }

    private void OnAdsStateChanged(AdsState adsState)
    {
        switch (adsState)
        {
            case AdsState.Config:
                IsEnabled = true;
                IsContentEnabled = true;
                RunIndicatorVisibility = false;
                ConfigIndicatorVisibility = true;
                return;
            case AdsState.Run:
                IsEnabled = true;
                IsContentEnabled = false;
                RunIndicatorVisibility = true;
                ConfigIndicatorVisibility = false;
                return;
            case AdsState.Idle:
                IsEnabled = false;
                IsContentEnabled = false;
                RunIndicatorVisibility = false;
                ConfigIndicatorVisibility = false;
                return;
        }
    }
    
    [RelayCommand]
    private void CreateProject()
    {
        DteSingleThread.Run(tcSysManager =>
        {
            tcSysManager.SaveProject();
            if (tcSysManager.GetPlcProject() is not { } plcProjectItem)
            {
                Logger.LogError(this, "No Plc project found");
                return;
            }

            HilGenerator.Update(tcSysManager, plcProjectItem);
            SilGenerator.UpdateAll(plcProjectItem);
            ProjectGenerator.Update(plcProjectItem);
            Logger.LogInfo(this, "Project update finished.");
        });
    }
    
    [RelayCommand]
    private void CreateTask()
    {
        DteSingleThread.Run(tcSysManager =>
        {
            TaskGenerator.CreateVariables(tcSysManager);
            Logger.LogInfo(this, "Project update finished.");
        });
    }
    
    [RelayCommand]
    private async Task CreateTemplate()
    {
        var inputField = new Avalonia.Controls.TextBox
        {
            Text = "DeviceName", 
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        
        var generator = new DeviceTemplate();

        if (!await MessageBox.Show(
                inputField,
                MessageBoxButton.OkCancel,
                MessageBoxImage.None,
                () => generator.SetName(inputField.Text)))
        {
            return;
        }
        
        generator.Create();
    }
    
    [RelayCommand]
    private async Task OpenSettings()
    {
        var settingsViewModel = new SettingsViewModel();
        var settings = new Views.Settings{DataContext = settingsViewModel};

        await MessageBox.Show(
            settings,
            MessageBoxButton.OkCancel,
            MessageBoxImage.None,
            settingsViewModel.Save);
    }

    public bool IsMenuOpened
    {
        get;
        set
        {
            field = value;
            NedId = $"NedId: {TcState.Singleton.AmsNetId}";
        }
    }
}