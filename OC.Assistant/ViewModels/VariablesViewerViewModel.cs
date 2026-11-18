using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Models;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.ViewModels;

/// <summary>
/// Shows the live values of all input and output variables of a plugin instance
/// and allows forcing them.
/// </summary>
public partial class VariablesViewerViewModel : ObservableObject, IDisposable
{
    private const string INPUTS = "Inputs";
    private const string OUTPUTS = "Outputs";

    private readonly Plugin _plugin;
    private readonly DispatcherTimer _timer;
    private readonly Dictionary<(bool, string, TcType), string> _forcedValues = new();
    private List<VariableViewModel> _variables = [];
    private IPluginController? _controller;
    private IReadOnlyList<IoVariable> _inputLayout = [];
    private IReadOnlyList<IoVariable> _outputLayout = [];

    public VariablesViewerViewModel(Plugin plugin)
    {
        _plugin = plugin;
        _plugin.Saved += PluginOnSaved;
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Background, (_, _) => Refresh());
        Refresh();
        _timer.Start();
    }

    public string Title => $"{_plugin.Name} - Variables";

    [ObservableProperty]
    public partial IReadOnlyList<VariableViewModel> Items { get; private set; } = [];

    [ObservableProperty]
    public partial string Filter { get; set; } = "";

    [ObservableProperty]
    public partial bool IsRunning { get; private set; }

    [ObservableProperty]
    public partial int ForcedCount { get; private set; }

    partial void OnFilterChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void ReleaseAll()
    {
        _forcedValues.Clear();
        _controller?.InputForces.Clear();
        _controller?.OutputForces.Clear();

        // Rows notify ForceChanged while being reset; the dictionary is already empty then.
        foreach (var variable in _variables.Where(x => x.IsForced))
        {
            variable.IsForced = false;
        }

        ForcedCount = 0;
    }

    private void PluginOnSaved() => OnPropertyChanged(nameof(Title));

    private void Refresh()
    {
        var controller = _plugin.PluginController;
        IsRunning = controller?.IsRunning == true;
        if (controller is null)
        {
            if (_variables.Count > 0) Rebuild(null, [], []);
            return;
        }

        var inputLayout = controller.InputStructure.Variables;
        var outputLayout = controller.OutputStructure.Variables;

        if (!ReferenceEquals(controller, _controller) ||
            !LayoutEquals(inputLayout, _inputLayout) ||
            !LayoutEquals(outputLayout, _outputLayout))
        {
            Rebuild(controller, inputLayout, outputLayout);
        }

        var inputBuffer = controller.InputBuffer;
        var outputBuffer = controller.OutputBuffer;

        foreach (var variable in _variables)
        {
            variable.Refresh(variable.IsInput ? inputBuffer : outputBuffer);
        }
    }

    private static bool LayoutEquals(IReadOnlyList<IoVariable> a, IReadOnlyList<IoVariable> b) =>
        ReferenceEquals(a, b) || a.SequenceEqual(b);

    private void Rebuild(
        IPluginController? controller,
        IReadOnlyList<IoVariable> inputLayout,
        IReadOnlyList<IoVariable> outputLayout)
    {
        _controller = controller;
        _inputLayout = inputLayout;
        _outputLayout = outputLayout;

        var variables = new List<VariableViewModel>();
        AddVariables(variables, true, inputLayout);
        AddVariables(variables, false, outputLayout);

        // Restore forced values. The force tables of the SDK are cleared when the layout changes,
        // or are empty when the plugin type has been replaced, so they are applied again here.
        foreach (var variable in variables)
        {
            if (_forcedValues.TryGetValue(variable.Key, out var value)) variable.Restore(value);
        }

        _variables = variables;
        UpdateForcedCount();
        ApplyFilter();
    }

    private void AddVariables(List<VariableViewModel> target, bool isInput, IReadOnlyList<IoVariable> layout)
    {
        var prefix = isInput ? INPUTS : OUTPUTS;
        Func<IIoForceTable?> getForces = isInput
            ? () => _controller?.InputForces
            : () => _controller?.OutputForces;

        foreach (var variable in layout)
        {
            if (variable.ArraySize == 0)
            {
                target.Add(new VariableViewModel(
                    isInput, $"{prefix}.{variable.Name}", variable.Type, variable.BitOffset,
                    getForces, OnForceChanged));
                continue;
            }

            for (var i = 0; i < variable.ArraySize; i++)
            {
                target.Add(new VariableViewModel(
                    isInput, $"{prefix}.{variable.Name}[{i}]", variable.Type,
                    variable.BitOffset + i * variable.Type.BitSize(),
                    getForces, OnForceChanged));
            }
        }
    }

    private void OnForceChanged(VariableViewModel variable)
    {
        if (variable.IsForced) _forcedValues[variable.Key] = variable.Value;
        else _forcedValues.Remove(variable.Key);
        UpdateForcedCount();
    }

    private void UpdateForcedCount() => ForcedCount = _variables.Count(x => x.IsForced);

    private void ApplyFilter()
    {
        var filter = Filter.Trim();
        Items = filter.Length == 0
            ? _variables
            : _variables.Where(x => x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Stops refreshing and releases all forced values.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _timer.Stop();
        _plugin.Saved -= PluginOnSaved;
        _forcedValues.Clear();
        _controller?.InputForces.Clear();
        _controller?.OutputForces.Clear();
    }
}
