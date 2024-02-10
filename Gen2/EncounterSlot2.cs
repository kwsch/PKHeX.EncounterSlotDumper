namespace PKHeX.EncounterSlotDumper;

/// <summary>
/// Generation 2 Wild Encounter Slot data
/// </summary>
/// <remarks>
/// Contains Time data which is present in Crystal origin data.
/// </remarks>
public sealed record EncounterSlot2
{
    public ushort Species { get; private set; }
    public byte LevelMin { get; private set; }
    public byte LevelMax { get; private set; }

    public int SlotNumber { get; set; }


    internal EncounterTime Time;

    public EncounterSlot2(byte species, byte min, byte max, byte slot)
    {
        Species = species;
        LevelMin = min;
        LevelMax = max;
        SlotNumber = slot;
    }

    public override string ToString() => $"{Time} - {base.ToString()}";

    /// <summary>
    /// Deserializes Gen2 Encounter Slots from data.
    /// </summary>
    /// <param name="data">Byte array containing complete slot data table.</param>
    /// <param name="ofs">Offset to start reading from.</param>
    /// <param name="count">Amount of slots to read.</param>
    /// <param name="type">Type of encounter slot table.</param>
    /// <returns>Array of encounter slots.</returns>
    public static EncounterSlot2[] ReadSlots(byte[] data, ref int ofs, int count, SlotType2 type)
    {
        var bump = type == SlotType2.Surf ? 4 : 0;
        var slots = new EncounterSlot2[count];
        for (int slot = 0; slot < count; slot++)
        {
            var min = data[ofs++];
            var species = data[ofs++];
            var max = (byte)(min + bump);
            slots[slot] = new EncounterSlot2(species, min, max, (byte)slot);
        }
        return slots;
    }

    internal void SetAsSpecial(byte species, byte level, EncounterTime time)
    {
        Species = species;
        LevelMin = level;
        LevelMax = level;
        Time = time;
    }
}
