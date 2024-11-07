using System.Net.Sockets;

namespace OC.Assistant.Sdk.TcpIp;

internal static class NetWorkStreamExtension
{
    internal static void Read(this NetworkStream stream, Message message)
    {
        var len = stream.Read(message.Buffer, 0, message.Buffer.Length);
        message.Length = len;
    }

    internal static void Write(this NetworkStream stream, Message message)
    {
        stream.Write(message.Buffer, 0, message.Length);
    }
}