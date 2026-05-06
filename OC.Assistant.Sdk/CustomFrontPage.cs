namespace OC.Assistant.Sdk;

/// <summary>
/// Classes with this attribute will be instantiated and added to the application's front page.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CustomFrontPage : Attribute;