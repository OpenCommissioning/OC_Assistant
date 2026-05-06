using TwinCAT.Ads;

namespace OC.Assistant.Twincat.Plugins.RecordData;

internal readonly struct Telegram(
    AmsAddress amsAddress,
    uint invokeId,
    uint indexGroup,
    uint indexOffset,
    uint length,
    byte[]? data)
{
    public AmsAddress AmsAddress { get; } = amsAddress;
    public uint InvokeId { get; } = invokeId;
    public uint IndexGroup { get; } = indexGroup;
    public uint IndexOffset { get; } = indexOffset;
    public uint Length { get; } = length;
    public byte[]? Data { get; } = data;

    /// <summary>
    /// 0x DD CC BB AA where<br/>
    /// AA = Port low byte<br/>
    /// BB = Port high byte<br/>
    /// CC = SubSlot<br/>
    /// DD = Slot<br/>
    /// </summary>
    public uint Key
    {
        get
        {
            var port = (ushort)AmsAddress.Port;
            var subSlot = (byte)IndexOffset;
            var slot = (byte)(IndexOffset >> 16);
            return port | (uint)(subSlot << 16) | (uint)(slot << 24);
        }
    }
}