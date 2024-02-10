using System;
using static PKHeX.EncounterSlotDumper.SlotType7;

namespace PKHeX.EncounterSlotDumper;

public enum SlotType7 : byte
{
    Standard = 0,
    SOS = 1,
}

public sealed record EncounterArea7
{
    public required ushort Location { get; init; }
    public required SlotType7 Type { get; set; }
    public required EncounterSlot7[] Slots { get; set; }

    public static EncounterArea7[] GetArray(byte[][] entries)
    {
        var data = new EncounterArea7[entries.Length];
        for (int i = 0; i < data.Length; i++)
        {
            var areaData = entries[i];
            var count = (areaData.Length - 2) / 4;
            var location = BitConverter.ToInt16(areaData, 0);
            var slots = new EncounterSlot7[count];
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

    private static void ReadSlots(EncounterSlot7[] slots, byte[] areaData)
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
