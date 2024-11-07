using System.Net;

namespace OC.Assistant.Sdk.TcpIp;

/// <summary>
/// Handling an error message.
/// </summary>
public delegate void ErrorHandler(string message); 

/// <summary>
/// Handling a message.
/// </summary>
public delegate void MessageHandler(byte[] buffer, int messageLength);

/// <summary>
/// Handling connection infos.
/// </summary>
public delegate void ConnectionHandler(EndPoint remoteEndPoint);