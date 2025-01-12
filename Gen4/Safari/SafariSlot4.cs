namespace PKHeX.EncounterSlotDumper;

// 8 bytes
public readonly record struct SafariSlot4
{
    public readonly ushort Species;
    public readonly byte Level;
    public readonly byte SlotNumber;

    public readonly byte StaticIndex;
    public readonly byte MagnetPullIndex;
    public readonly byte StaticCount;
    public readonly byte MagnetPullCount;

    public SafariSlot4(EncounterSlot4 slot)
    {
        Species = slot.Species;
        Level = slot.LevelMin;
        SlotNumber = slot.SlotNumber;
        StaticIndex = slot.StaticIndex;
        MagnetPullIndex = slot.MagnetPullIndex;
        StaticCount = slot.StaticCount;
        MagnetPullCount = slot.MagnetPullCount;
    }

    public EncounterSlot4 Inflate() => new()
    {
        Species = Species,
        LevelMin = Level,
        LevelMax = Level,
        SlotNumber = SlotNumber,
        MagnetPullCount = MagnetPullCount,
        MagnetPullIndex = MagnetPullIndex,
        StaticCount = StaticCount,
        StaticIndex = StaticIndex,
    };
}
