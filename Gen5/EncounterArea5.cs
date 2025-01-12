using System;
using static PKHeX.EncounterSlotDumper.SlotType5;

namespace PKHeX.EncounterSlotDumper;

public enum SlotType5 : byte
{
    Standard = 0,
    Grass = 1,
    Surf = 2,
    Super_Rod = 3,

    Swarm = 4,
    HiddenGrotto = 5,
}

public sealed record EncounterArea5
{
    public required ushort Location { get; init; }
    public SlotType5 Type { get; set; } = Standard;
    public required EncounterSlot5[] Slots { get; set; }

    public static EncounterArea5[] GetArray(byte[][] entries)
    {
        var data = new EncounterArea5[entries.Length];
        for (int i = 0; i < data.Length; i++)
        {
            var areaData = entries[i];
            var count = (areaData.Length - 2) / 4;
            var location = BitConverter.ToInt16(areaData, 0);
            var slots = new EncounterSlot5[count];
            ReadSlots(slots, areaData);
            data[i] = new()
            {
                Location = (ushort)location,
                Type = Standard,
                Slots = slots,
            };
        }
        return data;
    }

    private static void ReadSlots(EncounterSlot5[] slots, byte[] areaData)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int ofs = 2 + (i * 4);
            var SpecForm = BitConverter.ToUInt16(areaData, ofs);
            slots[i] = new()
            {
                Species = (ushort)(SpecForm & 0x7FF),
                Form = (byte)(SpecForm >> 11),
                LevelMin = areaData[ofs + 2],
                LevelMax = areaData[ofs + 3],
            };
        }
    }

    /// <summary>
    /// Gets the encounter areas for species with same level range and same slot type at same location
    /// </summary>
    /// <param name="species">List of species that exist in the Area.</param>
    /// <param name="lvls">Paired min and max levels of the encounter slots.</param>
    /// <param name="location">Location index of the encounter area.</param>
    /// <param name="t">Encounter slot type of the encounter area.</param>
    /// <returns>Encounter area with slots</returns>
    public static EncounterArea5 GetSimpleEncounterArea(ReadOnlySpan<ushort> species, ReadOnlySpan<byte> lvls,
        short location, SlotType5 t)
    {
        if ((lvls.Length & 1) != 0) // levels data not paired; expect multiple of 2
            throw new Exception(nameof(lvls));

        var count = species.Length * (lvls.Length / 2);
        var slots = new EncounterSlot5[count];
        int ctr = 0;
        foreach (var s in species)
        {
            for (int i = 0; i < lvls.Length;)
            {
                slots[ctr] = new()
                {
                    LevelMin = lvls[i++],
                    LevelMax = lvls[i++],
                    Species = s,
                    SlotNumber = (byte)ctr,
                };
                ctr++;
            }
        }
        return new EncounterArea5 { Location = (ushort)location, Slots = slots, Type = t };
    }
}
