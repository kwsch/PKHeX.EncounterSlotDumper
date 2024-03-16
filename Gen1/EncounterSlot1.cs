using System;

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
    /// <returns>Array of encounter slots.</returns>
    public static EncounterSlot1[] ReadSlots(ReadOnlySpan<byte> data, ref int ofs, int count)
    {
        var slots = new EncounterSlot1[count];
        for (int slot = 0; slot < count; slot++)
        {
            var min = data[ofs++];
            var species = data[ofs++];
            var max = min;
            slots[slot] = new EncounterSlot1(species, min, max, (byte)slot);
        }

        return slots;
    }
}
