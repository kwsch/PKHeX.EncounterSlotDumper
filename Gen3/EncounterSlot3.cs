using System;

namespace PKHeX.EncounterSlotDumper;

public record EncounterSlot3 : EncounterSlot34, IMagnetStatic, INumberedSlot
{
    public override required ushort Species { get; init; }
    public byte Form { get; set; }
    public byte LevelMin { get; set; }
    public byte LevelMax { get; set; }

    public byte StaticIndex { get; set; }
    public byte MagnetPullIndex { get; set; }
    public byte StaticCount { get; set; }
    public byte MagnetPullCount { get; set; }

    public byte SlotNumber { get; set; }
}

internal sealed record EncounterSlot3Swarm : EncounterSlot3
{
    public ReadOnlyMemory<ushort> Moves { get; }

    public EncounterSlot3Swarm(ReadOnlySpan<ushort> moves) => Moves = moves.ToArray();
}
