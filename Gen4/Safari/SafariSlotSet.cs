using System;
using System.Buffers.Binary;

namespace PKHeX.EncounterSlotDumper;

public sealed class SafariSlotSet
{
    public readonly SlotType4 Type;
    public readonly EncounterSlot4[] Day     = new EncounterSlot4[10];
    public readonly EncounterSlot4[] Morning = new EncounterSlot4[10];
    public readonly EncounterSlot4[] Night   = new EncounterSlot4[10];
    public readonly EncounterSlot4[] ExtraDay;
    public readonly EncounterSlot4[] ExtraMorning;
    public readonly EncounterSlot4[] ExtraNight;
    public readonly BlockRequirement[] ExtraBlocks;

    public SafariSlotSet(ReadOnlySpan<byte> data, SlotType4 type, int extraCount)
    {
        Type = type;

        GetSlots(ref data, Morning);
        GetSlots(ref data, Day);
        GetSlots(ref data, Night);
        GetSlots(ref data, ExtraMorning = new EncounterSlot4[extraCount]);
        GetSlots(ref data, ExtraDay     = new EncounterSlot4[extraCount]);
        GetSlots(ref data, ExtraNight   = new EncounterSlot4[extraCount]);
        GetBlocks(ref data, ExtraBlocks = new BlockRequirement[extraCount]);
    }

    private static void GetSlots(ref ReadOnlySpan<byte> data, EncounterSlot4[] result)
    {
        // Read Slots
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Get(data[..4], (byte)i);
            data = data[4..];
        }
    }

    private static void GetBlocks(ref ReadOnlySpan<byte> data, BlockRequirement[] result)
    {
        // Read Slots
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = GetRequirement(data[..4]);
            data = data[4..];
        }
    }

    private static BlockRequirement GetRequirement(ReadOnlySpan<byte> data) => new()
    {
        Block0 = data[0],
        Count0 = data[1],
        Block1 = data[2],
        Count1 = data[3],
    };

    private static EncounterSlot4 Get(ReadOnlySpan<byte> data, byte slot) => new()
    {
        Species = BinaryPrimitives.ReadUInt16LittleEndian(data[..2]),
        Level = data[2],
        SlotNumber = slot,
    };
}
