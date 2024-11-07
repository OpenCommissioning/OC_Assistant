using System.Xml.Linq;

namespace OC.Assistant.Sdk.Plugin;

internal class IoStructure(string rootName) : IIoStructure
{
    private int _bitSize;

    public int Length => _bitSize % 8 == 0 ? _bitSize / 8 : _bitSize / 8 + 1;

    public void AddVariable(string name, TcType type, int arraySize = 0)
    {
        if (type == TcType.Unknown) return;
        if (type == TcType.Bit && arraySize != 0) return;
        
        var varType = arraySize > 0 
            ? $"ARRAY [0..{arraySize - 1}] OF {type.Name()}" 
            : type.Name();
        XElement.Add(new XElement("Var", new XElement("Name", name), new XElement("Type", varType)));
        
        //Here we fill up to full byte-size if the current variable is not a bit
        var mod8 = _bitSize % 8;
        if (type != TcType.Bit && mod8 != 0)
        {
            _bitSize += 8 - mod8;
        }
        
        _bitSize += arraySize > 0 ? arraySize * type.BitSize() : type.BitSize();
    }

    public XElement XElement { get; } = new (rootName);

    public void Clear()
    {
        XElement.RemoveNodes();
        _bitSize = 0;
    }
}