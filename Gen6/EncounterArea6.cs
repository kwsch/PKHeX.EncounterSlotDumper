using System;
using static PKHeX.EncounterSlotDumper.SlotType6;

namespace PKHeX.EncounterSlotDumper;

public sealed record EncounterArea6
{
    public required ushort Location { get; init; }
    public required SlotType6 Type { get; init; }
    public required EncounterSlot6[] Slots { get; set; }

    public static EncounterArea6[] GetArray(byte[][] entries)
    {
        var data = new EncounterArea6[entries.Length];
        for (int i = 0; i < data.Length; i++)
        {
            var areaData = entries[i];
            var count = (areaData.Length - 2) / 4;
            var location = BitConverter.ToInt16(areaData, 0);
            var slots = new EncounterSlot6[count];
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

    private static void ReadSlots(EncounterSlot6[] slots, byte[] areaData)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int ofs = 2 + (i * 4);
            ushort SpecForm = BitConverter.ToUInt16(areaData, ofs);
            slots[i] = new()
            {
                Species = (ushort)(SpecForm & 0x7FF),
                Form = (byte)(SpecForm >> 11),
                LevelMin = areaData[ofs + 2],
                LevelMax = areaData[ofs + 3],
            };
        }
    }
}

public enum SlotType6 : byte
{
    Standard = 0,
    Grass = 1,
    Surf = 2,
    Old_Rod = 3,
    Good_Rod = 4,
    Super_Rod = 5,
    Rock_Smash = 6,

    Horde = 7,
    FriendSafari = 8,
}
