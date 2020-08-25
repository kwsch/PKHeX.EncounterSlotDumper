using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PKHeX.EncounterSlotDumper
{
    public static class Dumper7
    {
        public static void DumpGen7()
        {

        }

        public static void Write(IEnumerable<EncounterArea7> area, string name, string ident = "g6")
        {
            var serialized = area.Select(Write).ToArray();
            List<byte[]> unique = new List<byte[]>();
            foreach (var a in serialized)
            {
                if (unique.Any(z => z.SequenceEqual(a)))
                    continue;
                unique.Add(a);
            }

            var packed = BinLinker.Pack(unique.ToArray(), ident);
            File.WriteAllBytes(name, packed);
            Console.WriteLine($"Wrote {name} with {unique.Count} unique tables (originally {serialized.Length}).");
        }

        public static byte[] Write(EncounterArea7 area)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(area.Location);
            bw.Write((byte)area.Type);
            bw.Write((byte)0);

            foreach (var slot in area.Slots.Cast<EncounterSlot7>())
                WriteSlot(bw, slot);

            return ms.ToArray();
        }

        private static void WriteSlot(BinaryWriter bw, EncounterSlot7 slot)
        {
            bw.Write((ushort)(slot.Species | (slot.Form << 11)));
            bw.Write((byte)slot.LevelMin);
            bw.Write((byte)slot.LevelMax);
        }
    }
}