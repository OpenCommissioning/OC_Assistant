using System.Text.RegularExpressions;

namespace OC.Assistant.Generator.Profinet;

/// <summary>
/// Represents a PLC address containing a direction and an address value.
/// </summary>
/// <remarks>
/// The class parses and validates the provided address string based on certain rules.
/// </remarks>
public partial class PlcAddress
{
    /// Represents a PLC address used in Profinet communication.
    /// This class parses and stores information about a PLC address
    /// given in a specific format. The address is extracted using a
    /// regular expression and is decomposed into the components
    /// direction (I or Q) and the numerical address.
    public PlcAddress(string name)
    {
        var regex = AddressRegex();
        var match = regex.Match(name);
        if (!match.Success) return;

        Direction = match.Groups[1].Value;
        Address = int.Parse(match.Groups[2].Value);
    }

    /// <summary>
    /// Gets a value indicating whether the PLC address is valid.
    /// </summary>
    /// <remarks>
    /// The address is considered valid if it is greater than or equal to zero.
    /// </remarks>
    public bool IsValid => Address >= 0;

    /// <summary>
    /// Gets the direction of the PLC address.
    /// </summary>
    /// <remarks>
    /// The direction represents the type of PLC address. It is typically represented
    /// by the first character in the address string, such as "I" for input or "Q" for output.
    /// This property is initialized using a regex match when the <see cref="PlcAddress"/> constructor is invoked.
    /// </remarks>
    public string Direction { get; } = "";

    /// Represents the address of the PLC variable.
    /// The address is only considered valid if it contains a non-negative value.
    public int Address { get; } = -1;
    
    [GeneratedRegex(@"^([I,Q])(\d+)$")]
    private static partial Regex AddressRegex();
}