using System;

namespace PKHeX.EncounterSlotDumper;

/// <summary>
/// Coordinate / Index Relationship for a Generation 2 Headbutt Tree
/// </summary>
internal readonly struct TreeCoordinates
{
#if DEBUG
    private readonly byte X;
    private readonly byte Y;
#endif
    public readonly byte Index;

    public TreeCoordinates(in byte x, in byte y)
    {
#if DEBUG
        X = x;
        Y = y;
#endif
        Index = (byte)(((x * y) + x + y) / 5 % 10);
    }

#if DEBUG
    public override string ToString() => $"{Index} @ ({X:D2},{Y:D2})";
#endif
}

[Serializable]
public class TreeAreaListing
{
    public required TreeAreaInfo[] Table { get; init; }
}

[Serializable]
public class TreeAreaInfo
{
    public required byte Location { get; init; }
    public required Tree[] Valid { get; init; }
    public required Tree[] Invalid { get; init; }
}

[Serializable]
public class Tree
{
    public int X { get; set; }
    public int Y { get; set; }
}
