using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper
{
    public static class Dumper6
    {
        public static void DumpGen6()
        {
            var XSlots = GetEncounterTables("xy", Resources.encounter_x);
            var YSlots = GetEncounterTables("xy", Resources.encounter_y);
            var SlotsA = GetEncounterTables("ao", Resources.encounter_a);
            var SlotsO = GetEncounterTables("ao", Resources.encounter_o);

            static EncounterArea6[] GetEncounterTables(string ident, byte[] mini)
            {
                var data = BinLinker.Unpack(mini, ident);
                return EncounterArea32.GetArray<EncounterArea6, EncounterSlot6>(data);
            }

            MarkG6XYSlots(ref XSlots);
            MarkG6XYSlots(ref YSlots);
            XSlots = ArrayUtil.ConcatAll(XSlots, SlotsXYAlt);
            YSlots = ArrayUtil.ConcatAll(YSlots, SlotsXYAlt);

            MarkG6AOSlots(ref SlotsA);
            MarkG6AOSlots(ref SlotsO);

            Write(XSlots, "encounter_x.pkl", "xy");
            Write(YSlots, "encounter_y.pkl", "xy");
            Write(SlotsA, "encounter_a.pkl", "ao");
            Write(SlotsO, "encounter_o.pkl", "ao");
        }

        private static void MarkG6XYSlots(ref EncounterArea6[] Areas)
        {
            var extra = new List<EncounterArea6>();
            foreach (var area in Areas)
            {
                var horde = area.Slots.Skip(area.Slots.Length - 15).ToArray();
                var hordeA = new EncounterArea6 { Location = area.Location, Type = SlotType.Horde, Slots = horde };
                extra.Add(hordeA);
                area.Slots = area.Slots.Take(area.Slots.Length - horde.Length).ToArray();
            }

            // Pressure can cause different forms to appear with max level.
            foreach (var area in Areas)
            {
                bool hasFlabebe = area.Slots.Any(z => z.Species == (int)Species.Flabébé);
                if (!hasFlabebe)
                    continue;
                var flabebe = area.Slots.Where(z => z.Species == (int)Species.Flabébé).ToArray();
                var max = flabebe.Max(z => z.LevelMax);
                var forms = flabebe.Select(z => z.Form).Distinct().ToArray();
                // Add a pressure proc slot to the area slots.
                foreach (var form in forms)
                {
                    var slot = flabebe.First(z => z.Form == form);
                    if (slot.LevelMax == max)
                        continue;
                    var newslot = new EncounterSlot6 { Species = slot.Species, Form = slot.Form, LevelMin = max, LevelMax = max };
                    area.Slots = area.Slots.Append(newslot).ToArray();
                }
            }

            Areas = ArrayUtil.ConcatAll(Areas, extra.ToArray());
        }

        private static void MarkG6AOSlots(ref EncounterArea6[] Areas)
        {
            var extra = new List<EncounterArea6>();
            foreach (var area in Areas)
            {
                var rock = area.Slots.Skip(32).Take(5).ToArray();
                var rockA = new EncounterArea6 { Location = area.Location, Type = SlotType.Rock_Smash, Slots = rock };
                extra.Add(rockA);

                var horde = area.Slots.Skip(area.Slots.Length - 15).ToArray();
                var hordeA = new EncounterArea6 { Location = area.Location, Type = SlotType.Horde, Slots = horde };
                extra.Add(hordeA);
                area.Slots = area.Slots.Take(32).Concat(area.Slots.Skip(37)).ToArray();
                area.Slots = area.Slots.Take(area.Slots.Length - horde.Length).ToArray();
            }

            Areas = ArrayUtil.ConcatAll(Areas, extra.ToArray());
        }

        public static void Write(IEnumerable<EncounterArea6> area, string name, string ident = "g6")
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

        public static byte[] Write(EncounterArea6 area)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(area.Location);
            bw.Write((byte)area.Type);
            bw.Write((byte)0);

            foreach (var slot in area.Slots.Cast<EncounterSlot6>())
                WriteSlot(bw, slot);

            return ms.ToArray();
        }

        private static void WriteSlot(BinaryWriter bw, EncounterSlot6 slot)
        {
            bw.Write((ushort)(slot.Species | (slot.Form << 11)));
            bw.Write((byte)slot.LevelMin);
            bw.Write((byte)slot.LevelMax);
        }

        #region XY Alt Slots
        private static readonly EncounterArea6[] SlotsXYAlt =
        {
            new EncounterArea6 {
                Location = 104, // Victory Road
                Type = SlotType.Grass,
                Slots = new[]
                {
                    // Drops
                    new EncounterSlot6 { Species = 075, LevelMin = 57, LevelMax = 57, Form = 0 }, // Graveler
                    new EncounterSlot6 { Species = 168, LevelMin = 58, LevelMax = 59, Form = 0 }, // Ariados
                    new EncounterSlot6 { Species = 714, LevelMin = 57, LevelMax = 59, Form = 0 }, // Noibat

                    // Swoops
                    new EncounterSlot6 { Species = 022, LevelMin = 57, LevelMax = 59, Form = 0 }, // Fearow
                    new EncounterSlot6 { Species = 227, LevelMin = 57, LevelMax = 59, Form = 0 }, // Skarmory
                    new EncounterSlot6 { Species = 635, LevelMin = 59, LevelMax = 59, Form = 0 }, // Hydreigon
                },},
            new EncounterArea6 {
                Location = 34, // Route 6
                Type = SlotType.Grass,
                Slots = new[]
                {
                    // Rustling Bush
                    new EncounterSlot6 { Species = 543, LevelMin = 10, LevelMax = 12, Form = 0 }, // Venipede
                    new EncounterSlot6 { Species = 531, LevelMin = 10, LevelMax = 12, Form = 0 }, // Audino
                },},

            new EncounterArea6 { Location = 38, // Route 7
                Type = SlotType.Grass,
                Slots = new[]
                {
                    // Berry Field
                    new EncounterSlot6 { Species = 165, LevelMin = 14, LevelMax = 15, Form = 0 }, // Ledyba
                    new EncounterSlot6 { Species = 313, LevelMin = 14, LevelMax = 15, Form = 0 }, // Volbeat
                    new EncounterSlot6 { Species = 314, LevelMin = 14, LevelMax = 15, Form = 0 }, // Illumise
                    new EncounterSlot6 { Species = 412, LevelMin = 14, LevelMax = 15, Form = 0 }, // Burmy
                    new EncounterSlot6 { Species = 415, LevelMin = 14, LevelMax = 15, Form = 0 }, // Combee
                    new EncounterSlot6 { Species = 665, LevelMin = 14, LevelMax = 15, Form = 30 }, // Spewpa
                },},

            new EncounterArea6 { Location = 88, // Route 18
                Type = SlotType.Grass,
                Slots = new[]
                {
                    // Rustling Bush
                    new EncounterSlot6 { Species = 632, LevelMin = 44, LevelMax = 46, Form = 0 }, // Durant
                    new EncounterSlot6 { Species = 631, LevelMin = 45, LevelMax = 45, Form = 0 }, // Heatmor
                },},

            new EncounterArea6 { Location = 132, // Glittering Cave
                Type = SlotType.Grass,
                Slots = new[]
                {
                    // Drops
                    new EncounterSlot6 { Species = 527, LevelMin = 15, LevelMax = 17, Form = 0 }, // Woobat
                    new EncounterSlot6 { Species = 597, LevelMin = 15, LevelMax = 17, Form = 0 }, // Ferroseed
                },},

            new EncounterArea6 { Location = 56, // Reflection Cave
                Type = SlotType.Grass,
                Slots = new[]
                {
                    // Drops
                    new EncounterSlot6 { Species = 527, LevelMin = 21, LevelMax = 23, Form = 0 }, // Woobat
                    new EncounterSlot6 { Species = 597, LevelMin = 21, LevelMax = 23, Form = 0 }, // Ferroseed
                },},

            new EncounterArea6 { Location = 140, // Terminus Cave
                Type = SlotType.Grass,
                Slots = new[]
                {
                    // Drops
                    new EncounterSlot6 { Species = 168, LevelMin = 44, LevelMax = 46, Form = 0 }, // Ariados
                    new EncounterSlot6 { Species = 714, LevelMin = 44, LevelMax = 46, Form = 0 }, // Noibat
                },},
        };
        #endregion
    }
}