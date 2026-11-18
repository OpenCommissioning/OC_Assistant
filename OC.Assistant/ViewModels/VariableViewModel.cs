using CommunityToolkit.Mvvm.ComponentModel;
using OC.Assistant.Models;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.ViewModels;

/// <summary>
/// A single input or output variable of a plugin instance, used by the <see cref="VariablesViewerViewModel"/>.
/// Arrays are represented by one <see cref="VariableViewModel"/> per element.
/// </summary>
public partial class VariableViewModel : ObservableObject
{
    private readonly Func<IIoForceTable?> _getForces;
    private readonly Action<VariableViewModel> _forceChanged;
    private bool _refreshing;

    public VariableViewModel(
        bool isInput, string name, TcType tcType, int bitOffset,
        Func<IIoForceTable?> getForces, Action<VariableViewModel> forceChanged)
    {
        IsInput = isInput;
        Name = name;
        TcType = tcType;
        BitOffset = bitOffset;
        _getForces = getForces;
        _forceChanged = forceChanged;
    }

    public bool IsInput { get; }
    public string Name { get; }
    public TcType TcType { get; }
    public string Type => TcType.Name();
    public int BitOffset { get; }
    private int BitSize => TcType == TcType.Bit ? 1 : TcType.BitSize();

    /// <summary>
    /// Identifies the variable across structure rebuilds.
    /// </summary>
    public (bool, string, TcType) Key => (IsInput, Name, TcType);

    [ObservableProperty]
    public partial string Value { get; set; } = "";

    [ObservableProperty]
    public partial bool IsForced { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    /// <summary>
    /// Updates the <see cref="Value"/> from the given buffer, unless the variable is forced.
    /// </summary>
    public void Refresh(byte[] buffer)
    {
        if (IsForced) return;
        _refreshing = true;
        Value = IoValueCodec.Read(buffer, BitOffset, TcType);
        _refreshing = false;
    }

    /// <summary>
    /// Forces the variable with the given value without notifying the owner.
    /// </summary>
    public void Restore(string value)
    {
        _refreshing = true;
        Value = value;
        IsForced = true;
        _refreshing = false;
        ApplyForce();
    }

    /// <summary>
    /// True for <see cref="Sdk.TcType.Bit"/> and <see cref="Sdk.TcType.Bool"/>; the value is shown as a checkbox.
    /// </summary>
    public bool IsBoolean => TcType is TcType.Bit or TcType.Bool;

    /// <summary>
    /// Boolean view of <see cref="Value"/>, used by the checkbox.
    /// </summary>
    public bool BoolValue
    {
        get => Value == "TRUE";
        set => Value = value ? "TRUE" : "FALSE";
    }

    partial void OnValueChanged(string value)
    {
        if (IsBoolean) OnPropertyChanged(nameof(BoolValue));
        if (_refreshing || !IsForced) return;
        ApplyForce();
        _forceChanged(this);
    }

    partial void OnIsForcedChanged(bool value)
    {
        if (_refreshing) return;

        if (value)
        {
            // A checkbox can't show a parse error, so an unknown boolean value is forced as FALSE.
            if (IsBoolean && string.IsNullOrEmpty(Value))
            {
                _refreshing = true;
                Value = "FALSE";
                _refreshing = false;
            }

            ApplyForce();
        }
        else
        {
            HasError = false;
            _getForces()?.Remove(BitOffset);
        }

        _forceChanged(this);
    }

    private void ApplyForce()
    {
        // An invalid value keeps a previously forced value active; the row shows an error.
        if (!IoValueCodec.TryParse(Value, TcType, out var bytes))
        {
            HasError = true;
            return;
        }

        HasError = false;
        _getForces()?.Set(BitOffset, BitSize, bytes);
    }
}
