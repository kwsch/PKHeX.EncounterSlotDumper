using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper
{
    public static class Dumper2
    {
        public static readonly byte[] BCC_SlotRates = { 20, 20, 10, 10, 05, 05, 10, 10, 05, 05 };

        public static void DumpGen2()
        {
            var g = Resources.encounter_gold;
            var s = Resources.encounter_silver;
            var c = Resources.encounter_crystal;

            var ga = EncounterArea2.GetArray2GrassWater(g);
            var sa = EncounterArea2.GetArray2GrassWater(s);
            var ca = EncounterArea2.GetArray2GrassWater(c);

            var gh = Resources.encounter_gold_h;
            var sh = Resources.encounter_silver_h;
            var ch = Resources.encounter_crystal_h;
            var fish = Resources.encounter_gsc_f;

            var gha = EncounterArea2.GetArray2Headbutt(gh);
            var sha = EncounterArea2.GetArray2Headbutt(sh);
            var cha = EncounterArea2.GetArray2Headbutt(ch);
            var f = EncounterArea2.GetArray2Fishing(fish);

            // Copy met locations from Crystal's met locations (already pre-formatted)
            for (int i = 0; i < ca.Length; i++)
                ga[i].Location = sa[i].Location = ca[i].Location;
            for (int i = 0; i < cha.Length; i++)
                gha[i].Location = sha[i].Location = cha[i].Location;

            // GS has different swarm ordering. Just manually apply the correct met location IDs.
            var gs_swarm = new short[] { 18, 25, 44, 35, 35 };
            for (int i = ga.Length - gs_swarm.Length, j = 0; i < ga.Length; i++, j++)
                ga[i].Location = sa[i].Location = gs_swarm[j];

            // Strip out the no-tree headbutt areas.
            {
                var gl = gha.ToList();
                var sl = sha.ToList();
                var cl = cha.ToList();

                gl.RemoveAll(z => (z.Type & ~SlotType.Special) == SlotType.Headbutt && !Dumper2h.Trees.ContainsKey(z.Location));
                sl.RemoveAll(z => (z.Type & ~SlotType.Special) == SlotType.Headbutt && !Dumper2h.Trees.ContainsKey(z.Location));
                cl.RemoveAll(z => (z.Type & ~SlotType.Special) == SlotType.Headbutt && !Dumper2h.Trees.ContainsKey(z.Location));

                if (gha.Length != gl.Count || sha.Length != sl.Count || cha.Length != cl.Count)
                    throw new Exception();
            }

            var gr = ga.Concat(gha).Concat(f)
                .Concat(new[] {EncounterBCC_GSC })
                .OrderBy(z => z.Location).ThenBy(z => z.Type);
            var sr = sa.Concat(sha).Concat(f)
                .Concat(new[] { EncounterBCC_GSC })
                .OrderBy(z => z.Location).ThenBy(z => z.Type);
            var cr = ca.Concat(cha).Concat(f)
                .Concat(new[] { EncounterBCC_GSC })
                .OrderBy(z => z.Location).ThenBy(z => z.Type);

            Write(gr, "encounter_gold.pkl");
            Write(sr, "encounter_silver.pkl");
            Write(cr, "encounter_crystal.pkl");
        }

        private static readonly EncounterArea2 EncounterBCC_GSC = new EncounterArea2 {
            Location = 19,
            Type = SlotType.BugContest,
            Rates = new byte[] {20}, // 40 in tall grass
            Slots = new EncounterSlot[]
            {
                new EncounterSlot2(010, 07, 18, 00), // Caterpie
                new EncounterSlot2(013, 07, 18, 01), // Weedle
                new EncounterSlot2(011, 09, 18, 02), // Metapod
                new EncounterSlot2(014, 09, 18, 03), // Kakuna
                new EncounterSlot2(012, 12, 15, 04), // Butterfree
                new EncounterSlot2(015, 12, 15, 05), // Beedrill
                new EncounterSlot2(048, 10, 16, 06), // Venonat
                new EncounterSlot2(046, 10, 17, 07), // Paras
                new EncounterSlot2(123, 13, 14, 08), // Scyther
                new EncounterSlot2(127, 13, 14, 09), // Pinsir
            }
        };

        public static void Write(IEnumerable<EncounterArea2> areas, string name, string ident = "g2")
        {
            var serialized = areas.Select(Write).SelectMany(z => z).ToArray();
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

        public static IEnumerable<byte[]> Write(EncounterArea2 area)
        {
            var type = (area.Type) & (SlotType)0xF;
            if (type == SlotType.Grass)
            {
                var slotCount = area.Slots.Length / area.Rates.Length;
                for (var r = 0; r < area.Rates.Length; r++)
                {
                    var rate = area.Rates[r];

                    using var ms = new MemoryStream();
                    using var bw = new BinaryWriter(ms);

                    bw.Write((byte)area.Location);
                    int firstSlot = r * slotCount;
                    var first = (EncounterSlot2)area.Slots[firstSlot];
                    bw.Write((byte)first.Time);

                    bw.Write((byte)area.Type);
                    bw.Write(rate);

                    for (int i = r * slotCount; i < (r + 1) * slotCount; i++)
                    {
                        var slot = (EncounterSlot2)area.Slots[i];
                        WriteSlot(bw, slot);
                    }
                    yield return ms.ToArray();
                }
            }
            else if (area.Type == SlotType.Old_Rod || area.Type == SlotType.Good_Rod || area.Type == SlotType.Super_Rod)
            {
                if (area.Slots.Length == area.Rates.Length)
                {
                    yield return WriteTable(area);
                    yield break;
                }

                var types = area.Slots.Cast<EncounterSlot2>().Select(z => z.Time).Distinct().ToList();
                types.RemoveAll(z => z == EncounterTime.Any);

                foreach (var t in types)
                    yield return WriteTableOfTime(area, t);
            }
            else
            {
                yield return WriteTable(area);
            }
        }

        private static byte[] WriteTableOfTime(EncounterArea2 area, EncounterTime t)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            var slots = area.Slots.Cast<EncounterSlot2>()
                .Where(z => z.Time == EncounterTime.Any || z.Time == t).ToArray();

            bw.Write((byte) area.Location);
            bw.Write((byte) t);

            bw.Write((byte) area.Type);
            bw.Write((byte) 0xFF);

            foreach (byte b in area.Rates)
                bw.Write(b);
            foreach (var s in slots)
                WriteSlot(bw, s);
            return ms.ToArray();
        }

        private static byte[] WriteTable(EncounterArea2 area)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write((byte) area.Location);
            var first = (EncounterSlot2) area.Slots[0];
            bw.Write((byte) first.Time);

            var type = (byte) area.Type;
            bw.Write(type);
            if ((SlotType)(type & 0xF) == SlotType.Surf)
            {
                bw.Write(area.Rates[0]);
            }
            else if ((SlotType)type == SlotType.BugContest)
            {
                bw.Write(area.Rates[0]);
            }
            else
            {
                bw.Write((byte)0xFF);
                if (area.Rates.Length == 1)
                    throw new Exception();
                foreach (byte b in area.Rates)
                    bw.Write(b);
            }

            foreach (var s in area.Slots.Cast<EncounterSlot2>())
                WriteSlot(bw, s);
            return ms.ToArray();
        }

        private static void WriteSlot(BinaryWriter bw, EncounterSlot2 slot)
        {
            bw.Write((byte)slot.Species);
            bw.Write((byte)slot.SlotNumber);
            bw.Write((byte)slot.LevelMin);
            bw.Write((byte)slot.LevelMax);
        }
    }
}