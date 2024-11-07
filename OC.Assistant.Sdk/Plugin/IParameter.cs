namespace OC.Assistant.Sdk.Plugin;

internal interface IParameter
{
    string Name { get; }
    object? Value { get; set; }
    object? ToolTip { get; }
    string? FileFilter { get; }
}