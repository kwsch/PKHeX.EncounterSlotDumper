using System;

namespace PKHeX.EncounterSlotDumper;

public sealed record BlockRequirement
{
    public byte Block0 { get; init; }
    public byte Count0 { get; init; }
    public byte Block1 { get; init; }
    public byte Count1 { get; init; }

    public override string ToString()
    {
        var result = $"{(SafariBlockType4)Block0} x{Count0}";
        if (Block1 != 0)
            result += $" {(SafariBlockType4)Block1} x{Count1}";
        return result;
    }

    public bool IsSatisfied(ReadOnlySpan<byte> placed)
    {
        if (Block0 != 0 && placed[Block0] < Count0)
            return false;
        if (Block1 != 0 && placed[Block1] < Count1)
            return false;
        return true;
    }
}
