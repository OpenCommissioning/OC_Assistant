namespace OC.Assistant.Sdk;

internal readonly struct TcAdsIndex(uint iGrp, uint iOffs)
{
    public bool Valid { get; } = iGrp != 0;
    public uint IndexGroup { get; } = iGrp;
    public uint IndexOffset { get; } = iOffs;
}