﻿using OC.Assistant.Core.TwinCat;

namespace OC.Assistant.Core;

/// <summary>
/// Interface to implement events when a solution has been selected or closed.
/// </summary>
public interface IProjectSelector
{
    /// <summary>
    /// Is raised when a solution has been selected. 
    /// </summary>
    public event Action<TcDte>? DteSelected;
    
    /// <summary>
    /// Is raised when a xml file has been selected. 
    /// </summary>
    public event Action<string>? XmlSelected;
    
    /// <summary>
    /// Is raised when a connected solution has been closed. 
    /// </summary>
    public event Action? DteClosed;
}