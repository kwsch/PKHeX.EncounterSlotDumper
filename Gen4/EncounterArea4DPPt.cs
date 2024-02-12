using System;
using System.Collections.Generic;
using System.Linq;
using static PKHeX.EncounterSlotDumper.SlotType4;

namespace PKHeX.EncounterSlotDumper;

public sealed record EncounterArea4DPPt : EncounterArea4
{
    /// <summary>
    /// Gets the encounter areas with slot information from Generation 4 Diamond, Pearl and Platinum data.
    /// </summary>
    /// <param name="entries">Raw data, one byte array per encounter area</param>
    /// <param name="pt">Platinum flag (for Trophy Garden slot insertion)</param>
    /// <returns>Array of encounter areas.</returns>
    public static EncounterArea4DPPt[] GetArray4DPPt(byte[][] entries, bool pt = false)
    {
        return entries.SelectMany(z => GetArea4DPPt(z, pt)).Where(Area => Area.Slots.Length != 0).ToArray();
    }

    private static EncounterSlot4[] GetSlots4GrassDPPt(byte[] data, int ofs, int numslots)
    {
        var slots = new EncounterSlot4[numslots];

        for (int i = 0; i < numslots; i++)
        {
            var o = ofs + (i * 8);
            var level = data[o];
            var species = BitConverter.ToInt32(data, o + 4);
            slots[i] = new EncounterSlot4
            {
                LevelMax = level,
                LevelMin = level,
                Species = (ushort)species,
                SlotNumber = (byte)i,
            };
        }
        return slots;
    }

    private static void GetSlots4WaterFishingDPPt(EncounterArea4 area, byte[] data, int ofs, int numslots)
    {
        var slots = new List<EncounterSlot4>();
        for (int i = 0; i < numslots; i++)
        {
            // max, min, unused, unused, [32bit species]
            var species = BitConverter.ToInt32(data, ofs + 4 + (i * 8));
            if (species <= 0)
                continue;
            // Fishing and Surf slots without a species ID are not added
            // DPPt does not have fishing or surf swarms, and does not have any Rock Smash encounters.
            slots.Add(new EncounterSlot4
            {
                LevelMax = data[ofs + 0 + (i * 8)],
                LevelMin = data[ofs + 1 + (i * 8)],
                Species = (ushort)species,
                SlotNumber = (byte)i,
            });
        }

        area.Slots = [.. slots];
        EncounterUtil.MarkEncountersStaticMagnetPull(area, PersonalTable.HGSS); // same as DPPt personal
    }

    private static IEnumerable<EncounterArea4DPPt> GetArea4DPPt(byte[] data, bool pt = false)
    {
        var location = BitConverter.ToUInt16(data, 0x00);
        if (location > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(location), "Location ID is too large.");

        var GrassRate = BitConverter.ToInt32(data, 0x02);
        if (GrassRate > 0)
        {
            var slots = new List<EncounterSlot4>();
            var GrassSlots = GetSlots4GrassDPPt(data, 0x06, 12);
            //Swarming slots replace slots 0 and 1
            var swarm = GetSlots4GrassSlotReplace(data, 0x66, 4, GrassSlots, Legal.Slot4_Swarm);
            //Morning and Night slots replace slots 2 and 3
            var morning = GetSlots4GrassSlotReplace(data, 0x6E, 4, GrassSlots, Legal.Slot4_Time); // Morning
            var night = GetSlots4GrassSlotReplace(data, 0x76, 4, GrassSlots, Legal.Slot4_Time); // Night
            //Pokéradar slots replace slots 4,5,10 and 11
            //Pokéradar is marked with different slot type because it have different PID-IV generationn
            var radar = GetSlots4GrassSlotReplace(data, 0x7E, 4, GrassSlots, Legal.Slot4_Radar);

            //24 bytes padding

            //Dual Slots replace slots 8 and 9
            var ruby = GetSlots4GrassSlotReplace(data, 0xA6, 4, GrassSlots, Legal.Slot4_Dual); // Ruby
            var sapphire = GetSlots4GrassSlotReplace(data, 0xAE, 4, GrassSlots, Legal.Slot4_Dual); // Sapphire
            var emerald = GetSlots4GrassSlotReplace(data, 0xB6, 4, GrassSlots, Legal.Slot4_Dual); // Emerald
            var firered = GetSlots4GrassSlotReplace(data, 0xBE, 4, GrassSlots, Legal.Slot4_Dual); // FireRed
            var leafgreen = GetSlots4GrassSlotReplace(data, 0xC6, 4, GrassSlots, Legal.Slot4_Dual); // LeafGreen

            slots.AddRange(GrassSlots);
            slots.AddRange(swarm);
            slots.AddRange(morning);
            slots.AddRange(night);
            slots.AddRange(radar);
            slots.AddRange(ruby);
            slots.AddRange(sapphire);
            slots.AddRange(emerald);
            slots.AddRange(firered);
            slots.AddRange(leafgreen);

            // Permute Static-Magnet Pull combinations
            // [None/Swarm]-[None/Morning/Night]-[None/Radar]-[None/R/S/E/F/L] [None/TrophyGarden]
            // 2 * 3 * 2 * 6 = 72 different combinations of slots (more with trophy garden)
            var regular = new List<List<EncounterSlot4>> { GrassSlots.Where(z => z.SlotNumber is 6 or 7).ToList() }; // every other slot is in the product
            var pair0 = new List<List<EncounterSlot4>> { GrassSlots.Where(z => Legal.Slot4_Swarm.Contains(z.SlotNumber)).ToList() };
            var pair1 = new List<List<EncounterSlot4>> { GrassSlots.Where(z => Legal.Slot4_Time.Contains(z.SlotNumber)).ToList() };
            var pair2 = new List<List<EncounterSlot4>> { GrassSlots.Where(z => Legal.Slot4_Radar.Contains(z.SlotNumber)).ToList() };
            var pair3 = new List<List<EncounterSlot4>> { GrassSlots.Where(z => Legal.Slot4_Dual.Contains(z.SlotNumber)).ToList() };

            if (swarm.Count != 0) pair0.Add(swarm);
            if (morning.Count != 0) pair1.Add(morning);
            if (night.Count != 0) pair1.Add(night);
            if (radar.Count != 0) pair2.Add(radar);

            if (ruby.Count != 0) pair3.Add(ruby);
            if (sapphire.Count != 0) pair3.Add(sapphire);
            if (emerald.Count != 0) pair3.Add(emerald);
            if (firered.Count != 0) pair3.Add(firered);
            if (leafgreen.Count != 0) pair3.Add(leafgreen);

            if (location == 68) // Trophy Garden
                AddPermutationsTrophyGarden(regular, slots, pt);
            else if (location == 52) // Great Marsh
                AddPermutationsGreatMarsh(regular, slots, pt);

            var set = new[] { regular, pair0, pair1, pair2, pair3 };
            var product = set.CartesianProduct();
            var extra = MarkStaticMagnetExtras(product);
            slots.AddRange(extra);

            yield return new EncounterArea4DPPt
            {
                Type = Grass,
                Location = (byte)location,
                Rate = (byte)GrassRate,
                Slots = [.. slots], 
            };
        }

        var SurfRate = BitConverter.ToInt32(data, 0xCE);
        if (SurfRate > 0)
        {
            var area = new EncounterArea4DPPt {Location = (byte)location,
                Type = Surf,
                Rate = (byte)SurfRate,
                Slots = [],
            };
            GetSlots4WaterFishingDPPt(area, data, 0xD2, 5);
            yield return area;
        }

        //44 bytes padding

        var OldRate = BitConverter.ToInt32(data, 0x126);
        if (OldRate > 0)
        {
            var area = new EncounterArea4DPPt { Location = (byte)location, 
                Type = Old_Rod,
                Rate = (byte)OldRate,
                Slots = [],
            };
            GetSlots4WaterFishingDPPt(area, data, 0x12A, 5);
            yield return area;
        }

        var GoodRate = BitConverter.ToInt32(data, 0x152);
        if (GoodRate > 0)
        {
            var area = new EncounterArea4DPPt {Location = (byte)location,
                Type = Good_Rod, 
                Rate = (byte)GoodRate,
                Slots = [],
            };
            GetSlots4WaterFishingDPPt(area, data, 0x156, 5);
            yield return area;
        }

        var SuperRate = BitConverter.ToInt32(data, 0x17E);
        if (SuperRate > 0)
        {
            var area = new EncounterArea4DPPt {Location = (byte)location, 
                Type = Super_Rod, 
                Rate = (byte)SuperRate,
                Slots = [],
            };
            GetSlots4WaterFishingDPPt(area, data, 0x182, 5);
            yield return area;
        }
    }

    private static void AddPermutationsGreatMarsh(List<List<EncounterSlot4>> regular, List<EncounterSlot4> slots, bool pt)
    {
        // Occupy Slots 6 & 7
        var species = pt ? Encounters4.MarshPt : Encounters4.MarshDP;

        var replace = new List<EncounterSlot4>();
        var s6 = regular[0][0];
        var s7 = regular[0][1];
        foreach (var s in species)
        {
            // Retain existing level ranges.
            replace.Add(s6 with { Species = s });
            replace.Add(s7 with { Species = s });
        }
        AddReplacedArea(regular, slots, replace);
    }

    private static void AddPermutationsTrophyGarden(List<List<EncounterSlot4>> regular, List<EncounterSlot4> slots, bool pt)
    {
        // Occupy Slots 6 & 7
        var species = pt ? Encounters4.TrophyPt : Encounters4.TrophyDP;
        var replace = new List<EncounterSlot4>();
        var s6 = regular[0][0];
        var s7 = regular[0][1];
        foreach (var s in species)
        {
            // Retain existing level ranges.
            replace.Add(s6 with { Species = s });
            replace.Add(s7 with { Species = s });
        }
        AddReplacedArea(regular, slots, replace);
    }

    private static void AddReplacedArea(List<List<EncounterSlot4>> regular, List<EncounterSlot4> slots, List<EncounterSlot4> replace)
    {
        slots.AddRange(replace);
        // get all permutations of inhabitants
        var unified = regular[0].Concat(replace).ToArray();
        for (int i = 0; i < unified.Length; i++)
        {
            for (int j = i + 1; j < unified.Length; j++)
                regular.Add([unified[i], unified[j]]);
        }
    }

    public EncounterArea4DPPt[] Split(ReadOnlySpan<byte> locations)
    {
        var Areas = new EncounterArea4DPPt[locations.Length];
        for (int i = 0; i < locations.Length; i++)
            Areas[i] = this with { Location = locations[i] };
        return Areas;
    }
}

public static class DPEncounterExtensions
{
    public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
    {
        IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
        return sequences.Aggregate(
            emptyProduct,
            (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));
    }
}
