namespace OC.Assistant.Sdk;

/// <summary>
/// <see cref="Double"/> extension methods.
/// </summary>
public static class DoubleExtension
{
    /// <summary>
    /// Compares two <see cref="Double"/> values.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="compareValue">The value to compare.</param>
    /// <param name="tolerance">The tolerance of the comparison.</param>
    /// <returns>True if the two values differs more than the tolerance, otherwise false.</returns>
    public static bool DiffersFrom(this double value, double compareValue, double tolerance = 0.001D)
    {
        return Math.Abs(value - compareValue) > tolerance;
    }
}