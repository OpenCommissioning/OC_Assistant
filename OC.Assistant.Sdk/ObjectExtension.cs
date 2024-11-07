using System.ComponentModel;

namespace OC.Assistant.Sdk;

/// <summary>
/// <see cref="object"/> extension methods.
/// </summary>
internal static class ObjectExtension
{
    /// <summary>
    /// Converts an object into a given type.
    /// </summary>
    /// <param name="value">A generic value.</param>
    /// <param name="type">The given type to convert to.</param>
    /// <returns></returns>
    public static object? ConvertTo(this object? value, Type? type)
    {
        try
        {
            if (value is null || type is null) return null;
            if (value.GetType() == type) return value;
                
            // Nullable types, e.g. int, double, bool, string ...
            return Nullable.GetUnderlyingType(type) is not null ? TypeDescriptor.GetConverter(type).ConvertFrom(value) : Convert.ChangeType(value, type);
        }
        catch
        {
            Logger.LogWarning(typeof(ObjectExtension), $"Unable to convert value {value} to type {type}", true);
            return default;
        }
    }
}