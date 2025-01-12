using System;
using System.Collections.Generic;
using System.Linq;

namespace PKHeX.EncounterSlotDumper;

public sealed record EncounterArea7b
{
    public ushort Location { get; set; }
    public byte ToArea1 { get; set; }
    public byte ToArea2 { get; set; } // None have more than 2 areas to feed into.
    public EncounterSlot7b[] Slots { get; internal set; }

    public static EncounterArea7b[] GetAreas(byte[][] input)
    {
        var result = new EncounterArea7b[input.Length - NoSpawnAreas.Length];
        var count = 0; // Track how many were actually added
        foreach (byte[] areaData in input)
        {
            var area = new EncounterArea7b(areaData);

            // If it's one of the areas that don't use spawns, skip it.
            if (NoSpawnAreas.Contains(area.Location))
                continue;
            result[count] = area;
            count++;
        }
        return result;
    }

    private EncounterArea7b(byte[] data)
    {
        Location = BitConverter.ToUInt16(data, 0);

        // Record the areas this area can feed into.
        foreach (var (Area1, Area2) in AreaLinks)
        {
            if (Location != Area1)
                continue;

            if (ToArea1 == 0)
                ToArea1 = Area1;
            else if (ToArea2 == 0)
                ToArea2 = Area2;
            else
                throw new Exception($"Attempted to add more than 2 crossover areas to area {Location}!");
        }
        Slots = ReadSlots(data, ToArea1, ToArea2);
    }

    // Areas that shouldn't natively spawn Pokémon. The cities have sky spawns that don't work.
    private static readonly ushort[] NoSpawnAreas =
    [
        028, 029, 030, 031, 032, 033, 035, 036
    ];

    // List of areas that have at least one slot that crosses into another. From Area, To Area
    private static readonly List<(byte Area1, byte Area2)> AreaLinks =
    [
        (03, 28),
        (04, 30),
        (05, 06),
        (08, 33),
        (09, 34),
        (13, 33),
        (14, 15),
        (15, 14),
        (18, 34),
        (21, 22),
        (22, 36),
        (22, 21),
        (23, 28),
        (27, 26)
    ];

    private static EncounterSlot7b[] ReadSlots(byte[] data, byte toarea1, byte toarea2)
    {
        var loc = BitConverter.ToUInt16(data, 0);
        const int size = 4;
        int count = (data.Length - 2) / size;
        var slots = new EncounterSlot7b[count];
        for (int i = 0; i < slots.Length; i++)
        {
            int offset = 2 + (size * i);
            ushort SpecForm = BitConverter.ToUInt16(data, offset);
            var species = (ushort)(SpecForm & 0x3FF);
            // never any forms
            var min = data[offset + 2];
            var max = data[offset + 3];
            byte flags = GetCrossoverAreaFlags(species, min, max, loc, toarea1, toarea2);
            slots[i] = new EncounterSlot7b(species, min, max, flags);
        }
        return slots;
    }

    public byte[] WriteSlots()
    {
        const int size = 4;
        var data = new byte[4 + (size * Slots.Length)];
        BitConverter.GetBytes(Location).CopyTo(data, 0);
        data[2] = ToArea1;
        data[3] = ToArea2;

        for (int i = 0; i < Slots.Length; i++)
        {
            var slot = Slots[i];
            int offset = 4 + (size * i);
            data[offset]     = (byte)slot.Species; // None higher than Dragonite = 149
            data[offset + 1] = slot.CrossoverAreas;
            data[offset + 2] = slot.LevelMin;
            data[offset + 3] = slot.LevelMax;
        }
        return data;
    }

    // Checks if this slot should be allowed to crossover into either of this area's ToAreas.
    public static byte GetCrossoverAreaFlags(ushort species, byte min, byte max, ushort loc, byte toarea1, byte toarea2)
    {
        byte flags = 0;
        foreach (var cross in Crossover7b.AllCrossovers)
        {
            if (cross.Species != species)
                continue;
            if (cross.LevelMin != min)
                continue;
            if (cross.LevelMax != max)
                continue;
            if (cross.FromArea != loc)
                continue;

            if (cross.ToArea == toarea1)
                flags |= 1;
            else if (cross.ToArea == toarea2)
                flags |= 2;

            if (flags != 0) // Assume there should only be one match at most.
                return flags;
        }
        return flags;
    }
}
