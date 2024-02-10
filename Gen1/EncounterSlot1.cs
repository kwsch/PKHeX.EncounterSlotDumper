using static PKHeX.EncounterSlotDumper.SlotType1;

namespace PKHeX.EncounterSlotDumper;

/// <summary>
/// Generation 1 Wild Encounter Slot data
/// </summary>
public sealed record EncounterSlot1 : INumberedSlot
{
    public ushort Species { get; init; }
    public byte LevelMin { get; set; }
    public byte LevelMax { get; set; }
    public byte SlotNumber { get; set; }

    public EncounterSlot1(byte species, byte min, byte max, byte slot)
    {
        Species = species;
        LevelMin = min;
        LevelMax = max;
        SlotNumber = slot;
    }

    /// <summary>
    /// Deserializes Gen1 Encounter Slots from data.
    /// </summary>
    /// <param name="data">Byte array containing complete slot data table.</param>
    /// <param name="ofs">Offset to start reading from.</param>
    /// <param name="count">Amount of slots to read.</param>
    /// <param name="type">Type of encounter slot table.</param>
    /// <returns>Array of encounter slots.</returns>
    public static EncounterSlot1[] ReadSlots(byte[] data, ref int ofs, int count, SlotType1 type)
    {
        var bump = type == Surf ? 4 : 0;
        var slots = new EncounterSlot1[count];
        for (int slot = 0; slot < count; slot++)
        {
            var min = data[ofs++];
            var species = data[ofs++];
            var max = (byte)(min + bump);
            slots[slot] = new EncounterSlot1(species, min, max, (byte)slot);
        }

        return slots;
    }

    public static readonly EncounterArea1 FishOld_RBY = new()
    {
        Location = 0, // Any
        Type = Old_Rod,
        Rate = 100,
        Slots =
        [
            new EncounterSlot1(129, 05, 05, 0) // Magikarp
        ]
    };

    public static readonly EncounterArea1 FishGood_RBY = new()
    {
        Location = 0, // Any
        Type = Good_Rod,
        Rate = 100,
        Slots =
        [
            new EncounterSlot1(118, 10, 10, 0), // Goldeen
            new EncounterSlot1(060, 10, 10, 1) // Poliwag
        ]
    };
}
