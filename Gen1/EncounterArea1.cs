using System;
using System.Collections.Generic;
using static PKHeX.EncounterSlotDumper.SlotType1;

namespace PKHeX.EncounterSlotDumper;

public enum SlotType1 : byte
{
    Grass = 0,
    Surf = 1,
    Old_Rod = 2,
    Good_Rod = 3,
    Super_Rod = 4,
}

public sealed record EncounterArea1
{
    public required EncounterSlot1[] Slots;
    public byte Location { get; set; }

    /// <summary>
    /// Wild Encounter activity rate
    /// </summary>
    public int Rate { get; set; }

    public required SlotType1 Type { get; init; }

    private static EncounterSlot1[] ReadSlots1FishingYellow(byte[] data, ref int ofs, int count)
    {
        // Convert byte to actual number
        ReadOnlySpan<byte> levels = [0xFF, 0x15, 0x67, 0x1D, 0x3B, 0x5C, 0x72, 0x16, 0x71, 0x18, 0x00, 0x6D, 0x80];
        ReadOnlySpan<byte> g1DexIDs = [0x47, 0x6E, 0x18, 0x9B, 0x17, 0x4E, 0x8A, 0x5C, 0x5D, 0x9D, 0x9E, 0x1B, 0x85, 0x16, 0x58, 0x59];
        ReadOnlySpan<byte> speciesIDs = [060, 061, 072, 073, 090, 098, 099, 116, 117, 118, 119, 120, 129, 130, 147, 148];

        var slots = new EncounterSlot1[count];
        for (int slot = 0; slot < count; slot++)
        {
            var species = speciesIDs[g1DexIDs.IndexOf(data[ofs++])];
            var lvl = (byte)(levels.IndexOf(data[ofs++]) * 5);
            slots[slot] = new EncounterSlot1(species, lvl, lvl, (byte)slot);
        }

        return slots;
    }

    /// <summary>
    /// Gets the encounter areas with slot information from Generation 1 Grass/Water data.
    /// </summary>
    /// <param name="data">Input raw data.</param>
    /// <param name="count">Count of areas in the binary.</param>
    /// <returns>Array of encounter areas.</returns>
    public static EncounterArea1[] GetArray1GrassWater(byte[] data, int count)
    {
        var areas = new List<EncounterArea1>(count);
        for (int i = 0; i < count; i++)
        {
            int ptr = BitConverter.ToInt16(data, i * 2);
            var g = new EncounterArea1
            {
                Type = Grass,
                Location = (byte)i,
                Slots = [],
            };

            var gSlots = GetSlots1GrassWater(data, g, ref ptr);
            if (gSlots.Length > 0)
            {
                areas.Add(g);
                g.Slots = gSlots;
            }

            var w = new EncounterArea1
            {
                Type = Surf,
                Location = (byte)i,
                Slots = [],
            };
            var wSlots = GetSlots1GrassWater(data, w, ref ptr);
            if (wSlots.Length > 0)
            {
                areas.Add(w);
                w.Slots = wSlots;
            }
        }

        return [.. areas];
    }

    /// <summary>
    /// Gets the encounter areas with slot information from Pokémon Yellow (Generation 1) Fishing data.
    /// </summary>
    /// <param name="data">Input raw data.</param>
    /// <returns>Array of encounter areas.</returns>
    public static EncounterArea1[] GetArray1FishingYellow(byte[] data)
    {
        const int size = 9;
        int count = data.Length / size;
        EncounterArea1[] areas = new EncounterArea1[count];
        for (int i = 0; i < count; i++)
        {
            int ofs = (i * size) + 1;
            areas[i] = new EncounterArea1
            {
                Location = data[(i * size) + 0],
                Type = Super_Rod,
                Slots = ReadSlots1FishingYellow(data, ref ofs, 4)
            };
        }

        return areas;
    }

    /// <summary>
    /// Gets the encounter areas with slot information from Generation 1 Fishing data.
    /// </summary>
    /// <param name="data">Input raw data.</param>
    /// <param name="count">Count of areas in the binary.</param>
    /// <returns>Array of encounter areas.</returns>
    public static EncounterArea1[] GetArray1Fishing(byte[] data, int count)
    {
        var areas = new EncounterArea1[count];
        for (int i = 0; i < areas.Length; i++)
        {
            int loc = data[(i * 3) + 0];
            int ptr = BitConverter.ToInt16(data, (i * 3) + 1);
            areas[i] = new EncounterArea1
            {
                Location = (byte)loc,
                Type = Super_Rod,
                Slots = GetSlots1Fishing(data, ptr)
            };
        }

        return areas;
    }

    private static EncounterSlot1[] GetSlots1GrassWater(byte[] data, EncounterArea1 a, ref int ofs)
    {
        int rate = data[ofs++];
        a.Rate = rate;
        return rate == 0 ? [] : EncounterSlot1.ReadSlots(data, ref ofs, 10, Grass);
    }

    private static EncounterSlot1[] GetSlots1Fishing(byte[] data, int ofs)
    {
        int count = data[ofs++];
        return EncounterSlot1.ReadSlots(data, ref ofs, count, Super_Rod);
    }
}
