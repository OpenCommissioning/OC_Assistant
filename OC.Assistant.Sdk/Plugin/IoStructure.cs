using System.Xml.Linq;

namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Represents the input- or output-structure for a plugin.
/// </summary>
public class IoStructure(string rootName) : IIoStructure
{
    private int _bitSize;
    private readonly Lock _lock = new();
    private readonly List<IoVariable> _variables = [];
    private IoVariable[]? _snapshot;

    /// <summary>
    /// All variables of the current structure.
    /// </summary>
    public IReadOnlyList<IoVariable> Variables
    {
        get
        {
            lock (_lock) return _snapshot ??= _variables.ToArray();
        }
    }

    /// <summary>
    /// Size in bytes of the current structure.
    /// </summary>
    public int Length => _bitSize % 8 == 0 ? _bitSize / 8 : _bitSize / 8 + 1;

    /// <summary>
    /// Adds a variable to the structure.
    /// </summary>
    /// <param name="name">Name of the variable.</param>
    /// <param name="type">Type of the variable.</param>
    /// <param name="arraySize">No array when zero, otherwise size of the array.</param>
    public void AddVariable(string name, TcType type, int arraySize = 0)
    {
        if (type == TcType.Unknown) return;
        if (type == TcType.Bit && arraySize != 0) return;

        lock (_lock)
        {
            Add(name, type, arraySize);
        }
    }

    private void Add(string name, TcType type, int arraySize)
    {
        var varType = arraySize > 0 
            ? $"ARRAY [0..{arraySize - 1}] OF {type.Name()}" 
            : type.Name();
        
        XElement.Add(
            new XElement("Var", 
                new XElement("Name", name), 
                new XElement("Type", varType)));
        
        //Here we fill up to full byte-size if the current variable is not a bit
        var mod8 = _bitSize % 8;
        if (type != TcType.Bit && mod8 != 0)
        {
            _bitSize += 8 - mod8;
        }
        
        _variables.Add(new IoVariable(name, type, arraySize, _bitSize));
        _snapshot = null;
        _bitSize += arraySize > 0 ? arraySize * type.BitSize() : type.BitSize();
    }

    /// <summary>
    /// The current structure in xml-format.
    /// </summary>
    public XElement XElement { get; } = new (rootName);

    /// <summary>
    /// Removes all variables.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            XElement.RemoveNodes();
            _variables.Clear();
            _snapshot = null;
            _bitSize = 0;
        }
    }
}