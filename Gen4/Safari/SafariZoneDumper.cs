using System.IO;

namespace PKHeX.EncounterSlotDumper;

public static class SafariZoneDumper
{
    public static void DumpZones(SafariZone[] zones)
    {
        using var sw = new StreamWriter("safariBase.txt");
        for (int i = 0; i < zones.Length; i++)
        {
            var zone = zones[i];
            sw.WriteLine($"Safari Zone: {(SafariSubZone4)i}");
            WriteArea(zone.Grass, "Grass", zone.Header.Grass);
            WriteArea(zone.Surf, "Surf", zone.Header.Surf);
            WriteArea(zone.Old, "Old", zone.Header.Old);
            WriteArea(zone.Good, "Good", zone.Header.Good);
            WriteArea(zone.Super, "Super", zone.Header.Super);

            sw.WriteLine();
        }

        void WriteArea(SafariSlotSet table, string name, byte value)
        {
            sw.WriteLine($"{name} - {value}");
            sw.WriteLine($"{name} @ Morning");
            WriteSlots(table.Morning);
            sw.WriteLine($"{name} @ Day");
            WriteSlots(table.Day);
            sw.WriteLine($"{name} @ Night");
            WriteSlots(table.Night);

            sw.WriteLine($"{name} @ Extra Morning");
            WriteSlotsE(table.ExtraMorning, table.ExtraBlocks);
            sw.WriteLine($"{name} @ Extra Day");
            WriteSlotsE(table.ExtraDay, table.ExtraBlocks);
            sw.WriteLine($"{name} @ Extra Night");
            WriteSlotsE(table.ExtraNight, table.ExtraBlocks);
        }

        void WriteSlotsE(EncounterSlot4[] slots, BlockRequirement[] blocks)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                var block = blocks[i];
                sw.WriteLine($"{i}: {slot.LevelMin}-{slot.LevelMax} {(Species)slot.Species} {block}");
            }
        }

        void WriteSlots(EncounterSlot4[] slots)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                sw.WriteLine($"{i}: {slot.LevelMin}-{slot.LevelMax} {(Species)slot.Species}");
            }
        }
    }
}
