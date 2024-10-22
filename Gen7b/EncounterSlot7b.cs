namespace PKHeX.EncounterSlotDumper;

public sealed record EncounterSlot7b
{
    public ushort Species { get; }
    public byte Form { get; }
    public byte LevelMin { get; }
    public byte LevelMax { get; }
    public byte CrossoverAreas { get; } // Bits to indicate which of the areas in the area header this slot can feed to

    public EncounterSlot7b(ushort species, byte min, byte max, byte areas = 0)
    {
        Species = species;
        Form = 0;
        LevelMin = min;
        LevelMax = max;
        CrossoverAreas = areas;
    }
}
