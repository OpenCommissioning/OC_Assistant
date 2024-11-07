namespace OC.Assistant.Sdk.TcpIp;

/// <summary>
/// Message with an underlying buffer.
/// </summary>
public class Message
{
    private readonly byte[] _buffer;
        
    /// <summary>
    /// The length of the current message in bytes.
    /// </summary>
    public int Length { get; internal set; }
        
    /// <summary>
    /// Creates a new message buffer with the given size.
    /// </summary>
    /// <param name="size">The size of the message buffer in bytes.</param>
    public Message(int size = 2048)
    {
        _buffer = new byte[size];
    }
        
    /// <summary>
    /// The message buffer.
    /// </summary>
    public byte[] Buffer
    {
        get => _buffer;
        set
        {
            Length = Math.Min(_buffer.Length, value.Length);
            Array.Copy(value, 0, _buffer, 0, Length);
        }
    }

    /// <summary>
    /// Converts the message buffer to an <see cref="System.Text.Encoding.ASCII"/> string.
    /// </summary>
    /// <returns></returns>
    public new string ToString()
    {

            return System.Text.Encoding.ASCII.GetString(_buffer, 0, Length);
        
    }

    /// <summary>
    /// Converts a given string to the message buffer.
    /// </summary>
    /// <param name="value">The given <see cref="string"/> value.</param>
    public void GetString(string value)
    {
        Buffer = System.Text.Encoding.ASCII.GetBytes(value);
    }
}