using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static System.Buffers.Binary.BinaryPrimitives;
using static PKHeX.EncounterSlotDumper.SlotType2;

namespace PKHeX.EncounterSlotDumper;

[Flags]
public enum SlotType2 : byte
{
    Grass = 0,
    Surf = 1,
    Old_Rod = 2,
    Good_Rod = 3,
    Super_Rod = 4,
    Rock_Smash = 5,

    Headbutt = 6,
    HeadbuttSpecial = 7,
    BugContest = 8,

    // swarm may be a separate type, future?
    // no RNG checks yet...?
}

public sealed record EncounterArea2
{
    public required EncounterSlot2[] Slots { get; set; }
    public byte Location { get; set; }
    public required byte AreaRate { get; init; }
    public required SlotType2 Type { get; init; }
    public byte[] SlotRates { get; init; } = [];

    /// <summary>
    /// Gets the encounter areas with <see cref="EncounterSlot2"/> information from Generation 2 Grass/Water data.
    /// </summary>
    /// <param name="data">Input raw data.</param>
    /// <returns>Array of encounter areas.</returns>
    public static EncounterArea2[] GetArray2GrassWater(ReadOnlySpan<byte> data)
    {
        int ofs = 0;
        var areas = new List<EncounterArea2>();
        areas.AddRange(GetAreas2(data, ref ofs, 3, 7, Grass)); // Johto Grass
        areas.AddRange(GetAreas2(data, ref ofs, 1, 3, Surf)); // Johto Water
        areas.AddRange(GetAreas2(data, ref ofs, 3, 7, Grass)); // Kanto Grass
        areas.AddRange(GetAreas2(data, ref ofs, 1, 3, Surf)); // Kanto Water
        areas.AddRange(GetAreas2(data, ref ofs, 3, 7, Grass)); // Swarm Grass
        areas.AddRange(GetAreas2(data, ref ofs, 1, 3, Surf)); // Swarm Water

        // Strip out inaccessible areas.
        {
            for (var i = 0; i < areas.Count; i++)
            {
                var area = areas[i];
                // National Park or Route 14
                // If it is a Rod type, remove it. No fishing tiles accessible.
                if (area.Location is 19 or 76 && area.Type is Old_Rod or Good_Rod or Super_Rod)
                    areas.RemoveAt(i);
            }
        }

        return [.. areas];
    }

    // Fishing Tables are not associated to a single map; a map picks a table to use.
    // For all maps that use a table, create a new EncounterArea with reference to the table's slots.
    private static ReadOnlySpan<sbyte> FishGroupForMapID =>
    [
        -1,  1, -1,  0,  3,  3,  3, -1, 10,  3,  2, -1, -1,  2,  3,  0,
        -1, -1,  3, -1, -1, -1,  3, -1, -1, -1, -1,  0, -1, -1,  0,  9,
        1,  0,  2,  2, -1,  3,  7,  3, -1,  3,  4,  8,  2, -1,  2,  1,
        -1,  3, -1, -1, -1, -1, -1,  0,  2,  2, -1, -1,  3,  1, -1, -1,
        -1,  2, -1,  2, -1, -1, -1, -1, -1, -1, 10, 10, -1, -1, -1, -1,
        -1,  7,  0,  1, -1,  1,  1,  3, -1, -1, -1,  1,  1,  2,  3, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
    ];

    private static readonly (sbyte Group, byte LocationID)[] FishSwarm =
    [
        (5, 8), // Qwilfish
        (6, 39) // Remoraid
    ];

    /// <summary>
    /// Gets the encounter areas with <see cref="EncounterSlot2"/> information from Generation 2 Grass/Water data.
    /// </summary>
    /// <param name="data">Input raw data.</param>
    /// <returns>Array of encounter areas.</returns>
    public static EncounterArea2[] GetArray2Fishing(ReadOnlySpan<byte> data)
    {
        int ofs = 0;
        var f = GetAreas2Fishing(data, ref ofs);

        var areas = new List<EncounterArea2>();
        for (byte i = 0; i < FishGroupForMapID.Length; i++)
        {
            var group = FishGroupForMapID[i];
            if (group != -1)
                AddTableForLocation(group, i);
        }

        void AddTableForLocation(sbyte group, byte locationID)
        {
            foreach (var t in f.Where(z => z.Location == group))
            {
                var fake = (EncounterArea2)t.MemberwiseClone();
                fake.Location = locationID;
                areas.Add(fake);
            }
        }

        // Some maps have two tables. Fortunately, there's only a few. Add the second table.
        AddTableForLocation(0, 10); // Union Cave (2: Inside, 0: B2F Shore)
        AddTableForLocation(1, 27); // Olivine City (0: Harbor, 1: City)
        AddTableForLocation(3, 46); // Silver Cave (2: Inside, 3: Outside)

        foreach (var (group, locationId) in FishSwarm)
            AddTableForLocation(group, locationId);

        return [.. areas];
    }

    public static EncounterArea2[] GetArray2Headbutt(ReadOnlySpan<byte> data)
    {
        int ofs = 0;
        return GetAreas2Headbutt(data, ref ofs).ToArray();
    }

    private static EncounterArea2 GetSlots2Fishing(ReadOnlySpan<byte> data, ref int ofs, byte location, SlotType2 t)
    {
        // scan for count
        // slot set ends with final slot having 0xFF 0x** 0x**
        const int size = 3;
        var length = data[ofs..].IndexOf(byte.MaxValue) + size;
        var count = length / size;

        var rates = new byte[count];
        var slots = new EncounterSlot2[count];
        for (int i = 0; i < slots.Length; i++)
        {
            rates[i] = data[ofs++];
            var species = data[ofs++];
            var level = data[ofs++];
            slots[i] = new EncounterSlot2(species, level, level, (byte)i);
        }

        return new() { Location = location, AreaRate = 50, SlotRates = rates, Slots = slots, Type = t };
    }

    private static void GetSlots2HeadRock(ICollection<EncounterArea2> areas, byte location, ReadOnlySpan<byte> data, ref int ofs,
        [ConstantExpected] int tableCount)
    {
        // rate, species, level (3 bytes)
        // slot set ends in 0xFF (1 byte)
        if (tableCount == 1)
        {
            areas.Add(ReadHeadbuttArea(data, ref ofs, location, Rock_Smash));
        }
        else
        {
            areas.Add(ReadHeadbuttArea(data, ref ofs, location, Headbutt));
            areas.Add(ReadHeadbuttArea(data, ref ofs, location, HeadbuttSpecial));
        }
    }

    private static EncounterArea2 ReadHeadbuttArea(ReadOnlySpan<byte> data, ref int ofs, byte location, [ConstantExpected] SlotType2 type)
    {
        const int size = 3;
        int length = data[ofs..].IndexOf(byte.MaxValue);
        int count = length / size;

        var rates = new byte[count];

        var slots = new EncounterSlot2[count];
        for (int i = 0; i < count; i++)
        {
            rates[i] = data[ofs++];
            var species = data[ofs++];
            var level = data[ofs++];
            slots[i] = new EncounterSlot2(species, level, level, (byte)i);
        }

        ofs++; // 0xFF
        return new() { Location = location, AreaRate = 0, SlotRates = rates, Slots = slots, Type = type };
    }

    private static IEnumerable<EncounterArea2> GetAreas2(ReadOnlySpan<byte> data, ref int ofs,
        [ConstantExpected] int slotSets, [ConstantExpected] int slotCount,
        [ConstantExpected] SlotType2 t)
    {
        var areas = new List<EncounterArea2>();
        while (data[ofs] != 0xFF) // end
            AddSlots2GrassWater(data, areas, ref ofs, t, slotSets, slotCount);
        ofs++;
        return areas;
    }

    private static void AddSlots2GrassWater(ReadOnlySpan<byte> data, List<EncounterArea2> areas, ref int ofs,
        [ConstantExpected] SlotType2 t,
        [ConstantExpected] int slotSets, [ConstantExpected] int slotCount)
    {
        var x = data[ofs++]; // 0x00
        if (x > 30)
            throw new Exception("Invalid data format");

        var location = data[ofs++];
        var areaRates = data.Slice(ofs, slotSets);
        ofs += slotSets;

        for (int i = 0; i < areaRates.Length; i++)
        {
            var areaRate = areaRates[i];
            var slots = EncounterSlot2.ReadSlots(data, ref ofs, slotCount, t);

            if (areaRates.Length != 1) // Time of Day rates.
            {
                var time = i switch
                {
                    0 => EncounterTime.Morning,
                    1 => EncounterTime.Day,
                    2 => EncounterTime.Night,
                    _ => throw new ArgumentOutOfRangeException(),
                };
                foreach (var slot in slots)
                    slot.Time = time;
            }

            var area = new EncounterArea2 { Location = location, AreaRate = areaRate, Slots = slots, Type = t };
            areas.Add(area);
        }
    }

    private static List<EncounterArea2> GetAreas2Fishing(ReadOnlySpan<byte> data, ref int ofs)
    {
        byte areaIndex = 0;
        var areas = new List<EncounterArea2>();
        while (ofs != 0x18C)
        {
            areas.Add(GetSlots2Fishing(data, ref ofs, areaIndex, Old_Rod));
            areas.Add(GetSlots2Fishing(data, ref ofs, areaIndex, Good_Rod));
            areas.Add(GetSlots2Fishing(data, ref ofs, areaIndex, Super_Rod));
            _ = checked(++areaIndex);
        }

        // Read TimeFishGroups (two per entry)
        var dl = new List<TimeTemplate>();
        while (ofs < data.Length)
            dl.Add(new TimeTemplate(data[ofs++], data[ofs++], data[ofs++], data[ofs++]));

        // Add TimeSlots
        // A slot with species 0 will use time group {level}.
        for (int a = 0; a < areas.Count; a++)
        {
            var area = areas[a];
            var length = area.Slots.Length;
            for (int i = 0; i < length; i++)
            {
                var timeSlot = area.Slots[i];
                if (timeSlot.Species != 0)
                    continue;
                var group = timeSlot.LevelMin;

                // Mutate the current table into two areas.
                var dayTable = area.Slots;
                var nightTable = area.Slots.ToArray();
                var tg = dl[group];
                (dayTable[i], nightTable[i]) = tg.GetSlots(timeSlot);

                area.Slots = dayTable;
                areas.Insert(++a, area with { Slots = nightTable });
                break;
            }
        }

        return areas;
    }

    private readonly record struct TimeTemplate(byte SpeciesD, byte LevelD, byte SpeciesN, byte LevelN)
    {
        public (EncounterSlot2 Day, EncounterSlot2 Night) GetSlots(EncounterSlot2 slot)
        {
            var d = slot with { Species = SpeciesD, LevelMin = LevelD, LevelMax = LevelD, Time = EncounterTime.Morning |  EncounterTime.Day };
            var n = slot with { Species = SpeciesN, LevelMin = LevelN, LevelMax = LevelD, Time = EncounterTime.Night };
            return (d, n);
        }
    }

    private static IEnumerable<EncounterArea2> GetAreas2Headbutt(ReadOnlySpan<byte> data, ref int ofs)
    {
        // Read Location Table
        // 00, location, group
        var headLoc = new List<byte>();
        var headID = new List<byte>();
        while (data[ofs] != 0xFF)
        {
            var x = data[ofs++]; // 0x00
            if (x > 30)
                throw new Exception("Invalid data format");
            headLoc.Add(data[ofs++]);
            headID.Add(data[ofs++]);
        }
        ofs++; // 0xFF

        var rockLoc = new List<byte>();
        var rockID = new List<byte>();
        while (data[ofs] != 0xFF)
        {
            var x = data[ofs++]; // 0x00
            if (x > 30)
                throw new Exception("Invalid data format");
            rockLoc.Add(data[ofs++]);
            rockID.Add(data[ofs++]);
        }
        ofs++;
        ofs += 0x16; // jump over GetTreeMons

        // Read ptr table
        Span<ushort> ptr = stackalloc ushort[data.Length == 0x109 ? 6 : 9]; // GS : C
        for (int i = 0; i < ptr.Length; i++)
        {
            ptr[i] = ReadUInt16LittleEndian(data[ofs..]);
            ofs += 2;
        }

        var min = GetMin(ptr);
        int baseOffset = min - ofs;

        // Read Tables
        var head = new List<EncounterArea2>();
        var rock = new List<EncounterArea2>();
        int headCount = headLoc.Count;
        for (int i = 0; i < headCount; i++)
        {
            int o = ptr[headID[i]] - baseOffset;
            GetSlots2HeadRock(head, headLoc[i], data, ref o, 2);
        }
        for (int i = 0; i < rock.Count; i++)
        {
            int o = ptr[rockID[i]] - baseOffset;
            GetSlots2HeadRock(rock, rockLoc[i], data, ref o, 1);
        }

        return head.Concat(rock);
    }

    private static int GetMin(Span<ushort> ptr)
    {
        int min = int.MaxValue;
        foreach (var p in ptr)
            min = Math.Min(min, p);
        return min;
    }
}
