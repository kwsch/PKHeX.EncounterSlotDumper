using System;

public sealed class SafariHeader(ReadOnlySpan<byte> Data)
{
    public readonly byte Grass = Data[0]; // 10
    public readonly byte Surf = Data[1]; // 3
    public readonly byte Old = Data[2]; // 2
    public readonly byte Good = Data[3]; // 2
    public readonly byte Super = Data[4]; // 2
}
