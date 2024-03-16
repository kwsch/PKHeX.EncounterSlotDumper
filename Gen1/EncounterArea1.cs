using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static System.Buffers.Binary.BinaryPrimitives;
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
    public byte Location { get; set; }
    public required byte Rate { get; init; }
    public required SlotType1 Type { get; init; }
    public required EncounterSlot1[] Slots { get; init; }

    private static EncounterSlot1[] ReadSlots1FishingYellow(ReadOnlySpan<byte> data, ref int ofs, [ConstantExpected] int count)
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
    public static EncounterArea1[] GetArray1GrassWater(ReadOnlySpan<byte> data, [ConstantExpected] int count)
    {
        var areas = new List<EncounterArea1>(count);
        for (int i = 0; i < count; i++)
        {
            int ptr = ReadUInt16LittleEndian(data[(i * 2)..]);

            var gRate = data[ptr++];
            if (gRate != 0)
                areas.Add(ReadSlotsGW(data, ref ptr, i, gRate, Grass));
            var wRate = data[ptr++];
            if (wRate != 0)
                areas.Add(ReadSlotsGW(data, ref ptr, i, wRate, Surf));
        }

        return [.. areas];
    }

    private static EncounterArea1 ReadSlotsGW(ReadOnlySpan<byte> data, ref int ofs, int areaIndex, byte rate, [ConstantExpected] SlotType1 type)
        => new()
    {
        Location = (byte)areaIndex,
        Rate = rate,
        Type = type,
        Slots = GetSlots1GrassWater(data, ref ofs, type),
    };

    /// <summary>
    /// Gets the encounter areas with slot information from Pokémon Yellow (Generation 1) Fishing data.
    /// </summary>
    /// <param name="data">Input raw data.</param>
    /// <returns>Array of encounter areas.</returns>
    public static EncounterArea1[] GetArray1FishingYellow(ReadOnlySpan<byte> data)
    {
        const int size = 9;
        int count = data.Length / size;
        var areas = new EncounterArea1[count];
        for (int i = 0; i < count; i++)
        {
            int ofs = (i * size) + 1;
            areas[i] = new EncounterArea1
            {
                Rate = 0, // Not really a rate, done separately. Dialogue disjoint.
                Location = data[(i * size) + 0],
                Type = Super_Rod,
                Slots = ReadSlots1FishingYellow(data, ref ofs, 4),
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
    public static EncounterArea1[] GetArray1Fishing(ReadOnlySpan<byte> data, [ConstantExpected] int count)
    {
        var areas = new EncounterArea1[count];
        for (int i = 0; i < areas.Length; i++)
        {
            int loc = data[(i * 3) + 0];
            int ptr = ReadUInt16LittleEndian(data[((i * 3) + 1)..]);
            areas[i] = new EncounterArea1
            {
                Rate = 0, // Not really a rate, done separately. Dialogue disjoint.
                Location = (byte)loc,
                Type = Super_Rod,
                Slots = GetSlots1Fishing(data, ref ptr),
            };
        }

        return areas;
    }

    private static EncounterSlot1[] GetSlots1GrassWater(ReadOnlySpan<byte> data, ref int ofs, [ConstantExpected] SlotType1 type)
    {
        return EncounterSlot1.ReadSlots(data, ref ofs, 10);
    }

    private static EncounterSlot1[] GetSlots1Fishing(ReadOnlySpan<byte> data, ref int ofs)
    {
        var count = data[ofs++];
        return EncounterSlot1.ReadSlots(data, ref ofs, count);
    }

    public static readonly EncounterArea1 FishOld_RBY = new()
    {
        Location = 88, // Any, choose Pallet Town (FR/LG index)
        Type = Old_Rod,
        Rate = 0,
        Slots =
        [
            new EncounterSlot1(129, 05, 05, 0), // Magikarp
        ]
    };

    public static readonly EncounterArea1 FishGood_RBY = new()
    {
        Location = 88, // Any, choose Pallet Town (FR/LG index)
        Type = Good_Rod,
        Rate = 0,
        Slots =
        [
            new EncounterSlot1(118, 10, 10, 0), // Goldeen
            new EncounterSlot1(060, 10, 10, 1), // Poliwag
        ]
    };
}
