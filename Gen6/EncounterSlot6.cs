namespace PKHeX.EncounterSlotDumper;

public sealed record EncounterSlot6 : INumberedSlot
{
    public required ushort Species { get; set; }
    public byte Form { get; set; }
    public byte LevelMin { get; set; }
    public byte LevelMax { get; set; }

    public byte SlotNumber { get; set; }
}
