namespace OC.Assistant.Controls;

public partial class ProjectStateView
{
    public ProjectStateView()
    {
        InitializeComponent();
        OnDisconnected();
        
        AppControl.Instance.Connected += OnConnected;
        AppControl.Instance.Disconnected += OnDisconnected;
    }

    private void OnConnected(string projectFile)
        => ProjectLabel.Content = projectFile;
    
    private void OnDisconnected()
        => ProjectLabel.Content = null;
}