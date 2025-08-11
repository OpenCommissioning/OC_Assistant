using System.Text;

namespace OC.Assistant.Core;

internal class ApiMessage(byte[] payload)
{
    private byte[] Buffer { get; } = BuildMessage(payload);
    public const int HEADER_SIZE = 4;

    public new string ToString() => Encoding.UTF8.GetString(Buffer, HEADER_SIZE, Buffer.Length - HEADER_SIZE);
    
    public static bool IsHeaderValid(byte[] header)
    {
        if (header.Length != HEADER_SIZE)
        {
            return false;
        }

        return header[3] == (byte)(header[2] + (header[0] ^ header[1]));
    }

    public static int GetExpectedSize(byte[] header)
    {
        if (header.Length != HEADER_SIZE)
        {
            return 0;
        }
        return header[0] << 8 | header[1];
    }

    private static byte[] BuildHeader(byte[] payload)
    {
        var header = new byte[HEADER_SIZE];
        header[0] = (byte)((payload.Length & 0xFF00) >> 8);
        header[1] = (byte)(payload.Length & 0x00FF);
        header[2] = 0x01;
        header[3] = (byte)(header[2] + (header[0] ^ header[1]));
        return header;
    }
    
    private static byte[] BuildMessage(byte[] payload)
    {
        var header = BuildHeader(payload);
        var message = new byte[header.Length + payload.Length];
        Array.Copy(header, 0, message, 0, HEADER_SIZE);
        Array.Copy(payload, 0, message, HEADER_SIZE, payload.Length);
        return message;
    }
}