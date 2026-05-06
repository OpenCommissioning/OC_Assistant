using System.ComponentModel;
using System.Reflection;

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
            return null;
        }
    }
    
    /// <summary>
    /// Copies all public property values from one instances to another.
    /// </summary>
    /// <param name="self">The instance of type T to receive the values.</param>
    /// <param name="from">The second instance of type T to copy the values from.</param>
    /// <param name="ignore">An ignore list to exclude properties by name.</param>
    /// <typeparam name="T">The type of the instances.</typeparam>
    /// <returns>The modified instance.</returns>
    public static T CopyPropertyValues<T>(this T self, T from, params string[] ignore) where T : class
    {
        if (self is null || from is null) throw new NullReferenceException();
        var type = typeof(T);
        var ignoreList = new List<string>(ignore);
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (ignoreList.Contains(property.Name)) continue;
            var value = type.GetProperty(property.Name)?.GetValue(from, null);
            type.GetProperty(property.Name)?.SetValue(self, value);
        }
        return self;
    }
}