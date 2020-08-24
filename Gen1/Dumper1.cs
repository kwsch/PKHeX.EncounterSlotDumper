using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper
{
    public static class Dumper1
    {
        public static void DumpGen1()
        {
            var r = Resources.encounter_red;
            var b = Resources.encounter_blue;
            var y = Resources.encounter_yellow;

            var rbf = Resources.encounter_rb_f;
            var yf = Resources.encounter_yellow_f;

            var red_gw = EncounterArea1.GetArray1GrassWater(r, 248);
            var blu_gw = EncounterArea1.GetArray1GrassWater(b, 248);
            var ylw_gw = EncounterArea1.GetArray1GrassWater(y, 249);
            var rb_fish = EncounterArea1.GetArray1Fishing(rbf, 33);
            var ylw_fish = EncounterArea1.GetArray1FishingYellow(yf);

            foreach (var area in red_gw)
                area.Location = DumpUtil.RBYLocIndexes[area.Location];
            foreach (var area in blu_gw)
                area.Location = DumpUtil.RBYLocIndexes[area.Location];
            foreach (var area in ylw_gw)
                area.Location = DumpUtil.RBYLocIndexes[area.Location];

            for (var i = 0; i < rb_fish.Length; i++)
                rb_fish[i].Location = DumpUtil.RBFishIndexes[i];
            rb_fish = rb_fish.Where(z => z.Location != 0).ToArray(); // remove duplicate locations (cerulean gym same as cerulean city)
            for (var i = 0; i < ylw_fish.Length; i++)
                ylw_fish[i].Location = DumpUtil.YFishIndexes[i];

            var rb = red_gw.Concat(rb_fish).OrderBy(z => z.Location).ThenBy(z => z.Type);
            var bb = blu_gw.Concat(rb_fish).OrderBy(z => z.Location).ThenBy(z => z.Type);
            var yb = ylw_gw.Concat(ylw_fish).OrderBy(z => z.Location).ThenBy(z => z.Type);

            Write(rb, "encounter_red.pkl");
            Write(bb, "encounter_blue.pkl");
            Write(yb, "encounter_yellow.pkl");
        }

        public static void Write(IEnumerable<EncounterArea1> area, string name, string ident = "g1")
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

        public static byte[] Write(EncounterArea1 area)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(area.Location);
            bw.Write((byte)area.Type);
            bw.Write((byte)area.Rate);

            foreach (var slot in area.Slots.Cast<EncounterSlot1>())
                WriteSlot(bw, slot);

            return ms.ToArray();
        }

        private static void WriteSlot(BinaryWriter bw, EncounterSlot1 slot)
        {
            bw.Write((byte)slot.Species);
            bw.Write((byte)slot.SlotNumber);
            bw.Write((byte)slot.LevelMin);
            bw.Write((byte)slot.LevelMax);
        }
    }
}