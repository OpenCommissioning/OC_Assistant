using System.Xml.Linq;

namespace OC.Assistant.Sdk;

/// <summary>
/// Provides an event system that supports sending and receiving generic data.
/// </summary>
public static class EventSystem
{
    /// <summary>
    /// Represents the method that will handle app events.
    /// </summary>
    /// <param name="identifier">The data identifier.</param>
    /// <param name="payload">The data payload.</param>
    public delegate void AppDataHandler(string identifier, object? payload);
    
    /// <summary>
    /// Represents the method that will handle api events.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <param name="payload">The payload.</param>
    public delegate void ApiDataHandler(string identifier, XElement payload);
    
    /// <summary>
    /// Represents the method that will handle api request events.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <param name="payload">The payload.</param>
    /// <returns>The response payload.</returns>
    public delegate XElement ApiRequestHandler(string identifier, XElement payload);
    
    /// <summary>
    /// Invokes an app event.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <param name="payload">The payload.</param>
    /// <remarks>
    /// <c>app/connect</c>, projectFile as <see cref="string"/><br/>
    /// <c>app/disconnect</c><br/>
    /// <c>app/start</c>, channelType as <see cref="System.Type"/><br/>
    /// <c>app/stop</c>, channelType as <see cref="System.Type"/><br/>
    /// </remarks>
    public static void InvokeAppEvent(string identifier, object? payload = null) => AppDataReceived?.Invoke(identifier, payload);
    
    /// <summary>
    /// Event triggered when fixed or custom api data is received.
    /// </summary>
    /// <remarks>
    /// Fixed events:<br/>
    /// <c>app/connected</c>, projectFile as <see cref="string"/><br/>
    /// <c>app/disconnected</c>
    /// </remarks>
    public static event ApiDataHandler? ApiDataReceived;
    
    /// <summary>
    /// Event triggered when a custom api request is received.
    /// </summary>
    public static event ApiRequestHandler? ApiRequestReceived;
    
    /// <summary>
    /// Invokes an api event.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <param name="payload">The payload.</param>
    internal static void InvokeApiEvent(string identifier, XElement? payload = null) 
        => ApiDataReceived?.Invoke(identifier, payload ?? new XElement("Payload"));
    
    /// <summary>
    /// Invokes an api request event.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <param name="payload">The payload.</param>
    /// <returns>The response payload.</returns>
    internal static IEnumerable<XElement> InvokeApiRequest(string identifier, XElement payload)
    {
        if (ApiRequestReceived is null) yield break;
        foreach (var requestDelegate in ApiRequestReceived.GetInvocationList().Cast<ApiRequestHandler>())
        {
            yield return requestDelegate(identifier, payload);
        }
    }
    
    /// <summary>
    /// Event triggered when app data is received.
    /// </summary>
    internal static event AppDataHandler? AppDataReceived;
}