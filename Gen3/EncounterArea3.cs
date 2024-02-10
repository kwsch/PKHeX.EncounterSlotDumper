using System;
using System.Collections.Generic;
using static PKHeX.EncounterSlotDumper.SlotType3;

namespace PKHeX.EncounterSlotDumper;
public enum SlotType3 : byte
{
    Grass = 0,
    Surf = 1,
    Old_Rod = 2,
    Good_Rod = 3,
    Super_Rod = 4,
    Rock_Smash = 5,

    SwarmGrass50 = 6,
    SwarmFish50 = 7,
}

public sealed record EncounterArea3
{
    public required EncounterSlot3[] Slots;
    public required byte Location;

    public int Rate { get; set; }
    public required SlotType3 Type { get; set; }

    private static void GetSlots3(byte[] data, ref int ofs, int numslots, List<EncounterArea3> areas, byte location, SlotType3 t)
    {
        int rate = data[ofs];
        //1 byte padding
        if (rate > 0)
            ReadInSlots(data, ofs, numslots, areas, location, t, rate);
        ofs += 2 + (numslots * 4);
    }

    private static void ReadInSlots(byte[] data, int ofs, int numslots, List<EncounterArea3> areas, byte location, SlotType3 t, int rate)
    {
        var slots = new List<EncounterSlot3>();
        for (int i = 0; i < numslots; i++)
        {
            int o = ofs + (i * 4);
            var species = BitConverter.ToInt16(data, o + 4);
            if (species <= 0)
                continue;

            slots.Add(new EncounterSlot3
            {
                Species = (ushort)species,
                LevelMin = data[o + 2],
                LevelMax = data[o + 3],
                SlotNumber = (byte)i,
            });
        }

        var area = new EncounterArea3 { Location = location, Type = t, Rate = rate, Slots = [..slots] };
        areas.Add(area);
    }

    private static void GetSlots3Fishing(byte[] data, ref int ofs, int numslots, List<EncounterArea3> areas, byte location)
    {
        int Ratio = data[ofs];
        //1 byte padding
        if (Ratio > 0)
            ReadFishingSlots(data, ofs, numslots, areas, location);
        ofs += 2 + (numslots * 4);
    }

    private static void ReadFishingSlots(byte[] data, int ofs, int numslots, List<EncounterArea3> areas, byte location)
    {
        var o = new List<EncounterSlot3>();
        var g = new List<EncounterSlot3>();
        var s = new List<EncounterSlot3>();
        for (int i = 0; i < numslots; i++)
        {
            var species = BitConverter.ToInt16(data, ofs + 4 + (i * 4));
            if (species <= 0)
                continue;

            var slot = new EncounterSlot3
            {
                Species = (ushort)species,
                LevelMin = data[ofs + 2 + (i * 4)],
                LevelMax = data[ofs + 3 + (i * 4)],
            };

            if (i < 2)
            {
                o.Add(slot);
                slot.SlotNumber = (byte)i; // 0,1
            }
            else if (i < 5)
            {
                g.Add(slot);
                slot.SlotNumber = (byte)(i - 2); // 0,1,2
            }
            else
            {
                s.Add(slot);
                slot.SlotNumber = (byte)(i - 5); // 0,1,2,3,4
            }
        }

        var oa = new EncounterArea3 { Location = location, Type = Old_Rod, Slots = [.. o] };
        var ga = new EncounterArea3 { Location = location, Type = Good_Rod, Slots = [.. g] };
        var sa = new EncounterArea3 { Location = location, Type = Super_Rod, Slots = [.. s] };
        areas.Add(oa);
        areas.Add(ga);
        areas.Add(sa);
    }

    private static void GetArea3(byte[] data, List<EncounterArea3> areas)
    {
        var location = data[0];
        var HaveGrassSlots = data[1] == 1;
        var HaveSurfSlots = data[2] == 1;
        var HaveRockSmashSlots = data[3] == 1;
        var HaveFishingSlots = data[4] == 1;

        int offset = 5;
        if (HaveGrassSlots)
            GetSlots3(data, ref offset, 12, areas, location, Grass);
        if (HaveSurfSlots)
            GetSlots3(data, ref offset, 5, areas, location, Surf);
        if (HaveRockSmashSlots)
            GetSlots3(data, ref offset, 5, areas, location, Rock_Smash);
        if (HaveFishingSlots)
            GetSlots3Fishing(data, ref offset, 10, areas, location);
    }

    /// <summary>
    /// Gets the encounter areas with slot information from Generation 3 data.
    /// </summary>
    /// <param name="entries">Raw data, one byte array per encounter area</param>
    /// <returns>Array of encounter areas.</returns>
    public static EncounterArea3[] GetArray3(byte[][] entries)
    {
        var areas = new List<EncounterArea3>();
        foreach (var entry in entries)
            GetArea3(entry, areas);
        return [.. areas];
    }
}
