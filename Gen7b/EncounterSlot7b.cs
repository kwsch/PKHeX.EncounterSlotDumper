namespace PKHeX.EncounterSlotDumper;

public sealed record EncounterSlot7b
{
    public ushort Species { get; }
    public byte Form { get; }
    public byte LevelMin { get; }
    public byte LevelMax { get; }

    public EncounterSlot7b(ushort species, byte min, byte max)
    {
        Species = species;
        Form = 0;
        LevelMin = min;
        LevelMax = max;
    }
}
