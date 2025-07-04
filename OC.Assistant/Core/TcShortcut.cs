namespace OC.Assistant.Core;

/// <summary>
/// Table of available TwinCAT shortcuts.
/// </summary>
public struct TcShortcut
{
    /// <summary>
    /// Node I/O configuration.
    /// </summary>
    public const string NODE_IO_CONFIG = "TIIC";
    
    /// <summary>
    /// Node I/O configuration^I/O devices or <br/>
    /// Node I/O configuration TAB "I/O devices".
    /// </summary>
    public const string NODE_IO_DEVICES = "TIID";

    /// <summary>
    /// Node real-time configuration.
    /// </summary>
    public const string NODE_RT_CONFIG = "TIRC";

    /// <summary>
    /// Node real-time configuration^Additional tasks or <br/>
    /// Node real-time configuration TAB "Additional tasks".
    /// </summary>
    public const string NODE_RT_TASKS = "TIRT";

    /// <summary>
    /// Node real-time configuration^Real-time settings or <br/>
    /// Node real-time configuration TAB "Real-time settings".
    /// </summary>
    public const string NODE_RT_SETTINGS = "TIRS";

    /// <summary>
    /// Node PLC configuration.
    /// </summary>
    public const string NODE_PLC_CONFIG = "TIPC";

    /// <summary>
    /// Node NC configuration.
    /// </summary>
    public const string NODE_NC_CONFIG = "TINC";

    /// <summary>
    /// Node CNC configuration.
    /// </summary>
    public const string NODE_CNC_CONFIG = "TICC";

    /// <summary>
    /// Node CAM configuration.
    /// </summary>
    public const string NODE_CAM_CONFIG = "TIAC";

    /// <summary>
    /// Node axes.
    /// </summary>
    public const string NODE_AXES = "TING";

    /// <summary>
    /// Node tables.
    /// </summary>
    public const string NODE_TABLES = "TINT";

    /// <summary>
    /// Node SAF task.
    /// </summary>
    public const string NODE_SAF_TASK = "TINS";

    /// <summary>
    /// Terminal by id (only for KL bus).
    /// </summary>
    /// <returns><c>TIIT(id)</c></returns>
    public static string TERMINAL_BY_ID(string? id) => $"TIIT({id})";

    /// <summary>
    /// Box by id.
    /// </summary>
    /// <returns><c>TIIB(id)</c></returns>
    public static string BOX_BY_ID(string? id) => $"TIIB({id})";

    /// <summary>
    /// Device by id.
    /// </summary>
    /// <returns><c>TIIF(id)</c></returns>
    public static string DEVICE_BY_ID(string? id) => $"TIIF({id})";

    /// <summary>
    /// Terminal by name (only for KL bus).
    /// </summary>
    /// <returns><c>TIIT[name]</c></returns>
    public static string TERMINAL_BY_NAME(string? name) => $"TIIT[{name}]";

    /// <summary>
    /// Box by name.
    /// </summary>
    /// <returns><c>TIIB[name]</c></returns>
    public static string BOX_BY_NAME(string? name) => $"TIIB[{name}]";

    /// <summary>
    /// Device by name.
    /// </summary>
    /// <returns><c>TIIF[name]</c></returns>
    public static string DEVICE_BY_NAME(string? name) => $"TIIF[{name}]";
}