namespace OC.Assistant.Sdk;

/// <summary>
/// Classes with this attribute will be instantiated and added to the application's main menu.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CustomMenu : Attribute;