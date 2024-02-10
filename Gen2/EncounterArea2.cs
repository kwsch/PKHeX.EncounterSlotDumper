using System;
using System.Collections.Generic;
using System.Linq;
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
    public required EncounterSlot2[] Slots;
    public ushort Location { get; set; }

    public byte[] Rates { get; set; }
    public required SlotType2 Type { get; init; }

    /// <summary>
    /// Gets the encounter areas with <see cref="EncounterSlot2"/> information from Generation 2 Grass/Water data.
    /// </summary>
    /// <param name="data">Input raw data.</param>
    /// <returns>Array of encounter areas.</returns>
    public static EncounterArea2[] GetArray2GrassWater(byte[] data)
    {
        int ofs = 0;
        var areas = new List<EncounterArea2>();
        areas.AddRange(GetAreas2(data, ref ofs, Grass, 3, 7)); // Johto Grass
        areas.AddRange(GetAreas2(data, ref ofs, Surf, 1, 3)); // Johto Water
        areas.AddRange(GetAreas2(data, ref ofs, Grass, 3, 7)); // Kanto Grass
        areas.AddRange(GetAreas2(data, ref ofs, Surf, 1, 3)); // Kanto Water
        areas.AddRange(GetAreas2(data, ref ofs, Grass, 3, 7)); // Swarm Grass
        areas.AddRange(GetAreas2(data, ref ofs, Surf, 1, 3)); // Swarm Water

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

    private static readonly (byte Group, byte LocationID)[] FishSwarm =
    [
        (5, 8), // Qwilfish
        (6, 39) // Remoraid
    ];

    /// <summary>
    /// Gets the encounter areas with <see cref="EncounterSlot2"/> information from Generation 2 Grass/Water data.
    /// </summary>
    /// <param name="data">Input raw data.</param>
    /// <returns>Array of encounter areas.</returns>
    public static EncounterArea2[] GetArray2Fishing(byte[] data)
    {
        int ofs = 0;
        var f = GetAreas2Fishing(data, ref ofs);

        var areas = new List<EncounterArea2>();
        for (var i = 0; i < FishGroupForMapID.Length; i++)
        {
            var group = FishGroupForMapID[i];
            if (group != -1)
                AddTableForLocation(group, (byte)i);
        }

        void AddTableForLocation(int group, byte locationID)
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

    public static EncounterArea2[] GetArray2Headbutt(byte[] data)
    {
        int ofs = 0;
        return GetAreas2Headbutt(data, ref ofs).ToArray();
    }

    private static EncounterSlot2[] GetSlots2GrassWater(EncounterArea2 area, byte[] data, ref int ofs, SlotType2 t, int slotSets, int slotCount)
    {
        byte[] rates = new byte[slotSets];
        for (int i = 0; i < rates.Length; i++)
            rates[i] = data[ofs++];

        area.Rates = rates;
        var slots = EncounterSlot2.ReadSlots(data, ref ofs, slotSets * slotCount, t);
        if (slotSets <= 1)
            return slots;

        for (int i = 0; i < slotCount; i++)
        {
            slots[i].Time = EncounterTime.Morning;
        }
        for (int r = 1; r < slotSets; r++)
        {
            for (int i = 0; i < slotCount; i++)
            {
                int index = i + (r * slotCount);
                slots[index].SlotNumber = i;
                slots[index].Time = r == 1 ? EncounterTime.Day : EncounterTime.Night;
            }
        }

        return slots;
    }

    private static void GetSlots2Fishing(EncounterArea2 area, byte[] data, ref int ofs)
    {
        // slot set ends with final slot having 0xFF 0x** 0x**
        const int size = 3;
        int end = ofs; // scan for count
        while (data[end] != 0xFF)
            end += size;

        var count = ((end - ofs) / size) + 1;

        var rates = area.Rates = new byte[count];
        var slots = area.Slots = new EncounterSlot2[count];
        for (int i = 0; i < slots.Length; i++)
        {
            rates[i] = data[ofs++];
            var species = data[ofs++];
            var level = data[ofs++];
            slots[i] = new EncounterSlot2(species, level, level, (byte)i);
        }
    }

    private static void GetSlots2Headbutt(ICollection<EncounterArea2> areas, EncounterArea2 a, byte[] data, ref int ofs, int tableCount)
    {
        // slot set ends in 0xFF
        var slots = new List<EncounterSlot2>();
        var rates = new List<byte>();
        int slot = 0;
        while (tableCount != 0)
        {
            byte rate = data[ofs++];
            if (rate == 0xFF) // end of table
            {
                tableCount--;
                if (tableCount == 1)
                {
                    a.Slots = [.. slots];
                    a.Rates = [.. rates];

                    a = new EncounterArea2 { Location = a.Location, Type = HeadbuttSpecial, Slots = [] };
                    slots.Clear();
                    rates.Clear();
                    areas.Add(a);
                }
                continue;
            }

            var species = data[ofs++];
            var level = data[ofs++];
            rates.Add(rate);
            slots.Add(new EncounterSlot2(species, level, level, (byte)(slot++)));
        }
        a.Slots = [.. slots];
        a.Rates = [.. rates];
    }

    private static IEnumerable<EncounterArea2> GetAreas2(byte[] data, ref int ofs, SlotType2 t, int slotSets, int slotCount)
    {
        var areas = new List<EncounterArea2>();
        while (data[ofs] != 0xFF) // end
        {
            var location = data[ofs++] << 8 | data[ofs++];
            var area = new EncounterArea2 { Location = (ushort)location, Type = t, Slots = [] };
            var slots = GetSlots2GrassWater(area, data, ref ofs, t, slotSets, slotCount);
            area.Slots = slots;
            areas.Add(area);
        }
        ofs++;
        return areas;
    }

    private static List<EncounterArea2> GetAreas2Fishing(byte[] data, ref int ofs)
    {
        byte a = 0;
        var areas = new List<EncounterArea2>();
        while (ofs != 0x18C)
        {
            var aOld = new EncounterArea2 { Location = a, Type = Old_Rod, Slots = [] };
            var aGood = new EncounterArea2 { Location = a, Type = Good_Rod, Slots = [] };
            var aSuper = new EncounterArea2 { Location = a, Type = Super_Rod, Slots = [] };
            GetSlots2Fishing(aOld, data, ref ofs);
            GetSlots2Fishing(aGood, data, ref ofs);
            GetSlots2Fishing(aSuper, data, ref ofs);

            areas.Add(aOld);
            areas.Add(aGood);
            areas.Add(aSuper);
            _ = checked(++a);
        }

        // Read TimeFishGroups
        var dl = new List<SlotTemplate>();
        while (ofs < data.Length)
            dl.Add(new SlotTemplate(data[ofs++], data[ofs++]));

        // Add TimeSlots
        foreach (var area in areas)
        {
            var slots = area.Slots;
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot.Species != 0)
                    continue;

                Array.Resize(ref slots, slots.Length + 1);
                Array.Copy(slots, i, slots, i + 1, slots.Length - i - 1); // shift slots down
                slots[i + 1] = slot with { }; // differentiate copied slot

                int index = slot.LevelMin * 2;
                for (int j = 0; j < 2; j++) // load special slot info
                {
                    var s = slots[i + j];
                    var species = dl[index + j].Species;
                    var level = dl[index + j].Level;
                    var time = j == 0 ? EncounterTime.Morning | EncounterTime.Day : EncounterTime.Night;
                    s.SetAsSpecial(species, level, time);
                }
            }
            area.Slots = slots;
        }
        return areas;
    }

    private readonly struct SlotTemplate
    {
        public readonly byte Species;
        public readonly byte Level;

        public SlotTemplate(byte species, byte level)
        {
            Species = species;
            Level = level;
        }
    }

    private static IEnumerable<EncounterArea2> GetAreas2Headbutt(byte[] data, ref int ofs)
    {
        // Read Location Table
        var head = new List<EncounterArea2>();
        var headID = new List<int>();
        while (data[ofs] != 0xFF)
        {
            head.Add(new EncounterArea2
            {
                Location = (ushort)((data[ofs++] << 8) | data[ofs++]),
                Type = Headbutt,
                Slots = [],
            });
            headID.Add(data[ofs++]);
        }
        ofs++;

        var rock = new List<EncounterArea2>();
        var rockID = new List<int>();
        while (data[ofs] != 0xFF)
        {
            rock.Add(new EncounterArea2
            {
                Location = (ushort)((data[ofs++] << 8) | data[ofs++]),
                Type = Rock_Smash,
                Slots = [],
            });
            rockID.Add(data[ofs++]);
        }
        ofs++;
        ofs += 0x16; // jump over GetTreeMons

        // Read ptr table
        int[] ptr = new int[data.Length == 0x109 ? 6 : 9]; // GS : C
        for (int i = 0; i < ptr.Length; i++)
            ptr[i] = data[ofs++] | (data[ofs++] << 8);

        int baseOffset = ptr.Min() - ofs;

        // Read Tables
        int headCount = head.Count;
        for (int i = 0; i < headCount; i++)
        {
            int o = ptr[headID[i]] - baseOffset;
            GetSlots2Headbutt(head, head[i], data, ref o, 2);
        }
        for (int i = 0; i < rock.Count; i++)
        {
            int o = ptr[rockID[i]] - baseOffset;
            GetSlots2Headbutt(rock, rock[i], data, ref o, 1);
        }

        return head.Concat(rock);
    }
}
