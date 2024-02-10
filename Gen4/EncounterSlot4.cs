namespace PKHeX.EncounterSlotDumper;

public sealed record EncounterSlot4 : EncounterSlot34, IMagnetStatic, INumberedSlot
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

    public byte Level
    {
        set => LevelMin = LevelMax = value;
    }
}
