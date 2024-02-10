using System;
using System.Diagnostics;

namespace PKHeX.EncounterSlotDumper;

public sealed record EncounterArea7b
{
    public ushort Location { get; set; }
    public EncounterSlot7b[] Slots { get; internal set; }

    public static EncounterArea7b[] GetAreas(byte[][] input)
    {
        var result = new EncounterArea7b[input.Length];
        for (int i = 0; i < input.Length; i++)
            result[i] = new EncounterArea7b(input[i]);
        return result;
    }

    private EncounterArea7b(byte[] data)
    {
        Location = BitConverter.ToUInt16(data, 0);
        Slots = ReadSlots(data);
    }

    private EncounterSlot7b[] ReadSlots(byte[] data)
    {
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
            slots[i] = new EncounterSlot7b(species, min, max);
        }
        return slots;
    }

    public byte[] WriteSlots()
    {
        const int size = 4;
        var data = new byte[2 + (size * Slots.Length)];
        BitConverter.GetBytes(Location).CopyTo(data, 0);

        for (int i = 0; i < Slots.Length; i++)
        {
            var slot = Slots[i];
            int offset = 2 + (size * i);
            ushort SpecForm = (ushort)(slot.Species | (slot.Form << 11));
            Debug.Assert(SpecForm < 0x3FF);
            BitConverter.GetBytes(SpecForm).CopyTo(data, offset);
            data[offset + 2] = slot.LevelMin;
            data[offset + 3] = slot.LevelMax;
        }
        return data;
    }
}
