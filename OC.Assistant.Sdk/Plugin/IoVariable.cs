namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Describes a variable of an <see cref="IIoStructure"/>.
/// </summary>
/// <param name="Name">The name of the variable.</param>
/// <param name="Type">The <see cref="TcType"/> of the variable or of the array elements.</param>
/// <param name="ArraySize">No array when zero, otherwise size of the array.</param>
/// <param name="BitOffset">The offset in bits relative to the start of the structure.</param>
public readonly record struct IoVariable(string Name, TcType Type, int ArraySize, int BitOffset);
