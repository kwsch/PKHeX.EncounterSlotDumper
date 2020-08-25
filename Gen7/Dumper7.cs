using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static PKHeX.EncounterSlotDumper.Properties.Resources;

namespace PKHeX.EncounterSlotDumper
{
    public static class Dumper7
    {
        public static void DumpGen7()
        {
            static EncounterArea7[] GetEncounterTables(string ident, byte[] mini)
            {
                var data = BinLinker.Unpack(mini, ident);
                return EncounterArea32.GetArray<EncounterArea7, EncounterSlot7>(data);
            }

            var REG_SN = GetEncounterTables("sm", encounter_sn);
            var REG_MN = GetEncounterTables("sm", encounter_mn);
            var SOS_SN = GetEncounterTables("sm", encounter_sn_sos);
            var SOS_MN = GetEncounterTables("sm", encounter_mn_sos);

            var REG_US = GetEncounterTables("uu", encounter_us);
            var REG_UM = GetEncounterTables("uu", encounter_um);
            var SOS_US = GetEncounterTables("uu", encounter_us_sos);
            var SOS_UM = GetEncounterTables("uu", encounter_um_sos);

            MarkAreasAsSOS(ref SOS_SN);
            MarkAreasAsSOS(ref SOS_MN);

            MarkAreasAsSOS(ref SOS_US);
            MarkAreasAsSOS(ref SOS_UM);

            int[] pelagoMin = { 1, 11, 21, 37, 49 };
            InitializePelagoSM(pelagoMin, out var p_sn, out var p_mn);
            InitializePelagoUltra(pelagoMin, out var p_us, out var p_um);

            var SlotsSN = ArrayUtil.ConcatAll(REG_SN, SOS_SN, p_sn);
            var SlotsMN = ArrayUtil.ConcatAll(REG_MN, SOS_MN, p_mn);

            var SlotsUS = ArrayUtil.ConcatAll(REG_US, SOS_US, p_us);
            var SlotsUM = ArrayUtil.ConcatAll(REG_UM, SOS_UM, p_um);

            Write(SlotsSN, "encounter_sn.pkl", "sm");
            Write(SlotsMN, "encounter_mn.pkl", "sm");

            Write(SlotsUS, "encounter_us.pkl", "uu");
            Write(SlotsUM, "encounter_um.pkl", "uu");
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

        private static void MarkAreasAsSOS(ref EncounterArea7[] Areas)
        {
            foreach (var area in Areas)
                area.Type = SlotType.SOS;
        }

        private static void InitializePelagoSM(int[] minLevels, out EncounterArea7[] sn, out EncounterArea7[] mn)
        {
            int[][] speciesSM =
            {
                new[] {627/*SN*/, 021, 041, 090, 278, 731}, // 1-7
                new[] {064, 081, 092, 198, 426, 703},       // 11-17
                new[] {060, 120, 127, 661, 709, 771},       // 21-27
                new[] {227, 375, 707},                      // 37-43
                new[] {123, 131, 429, 587},                 // 49-55
            };
            sn = GetPelagoArea(speciesSM, minLevels);
            speciesSM[0][0] = 629; // Rufflet -> Vullaby
            mn = GetPelagoArea(speciesSM, minLevels);
        }

        private static void InitializePelagoUltra(int[] minLevels, out EncounterArea7[] us, out EncounterArea7[] um)
        {
            int[][] speciesUU =
            {
                new[] {731, 278, 041, 742, 086},        // 1-7
                new[] {079, 120, 222, 122, 180, 124},   // 11-17
                new[] {127, 177, 764, 163, 771, 701},   // 21-27
                new[] {131, 354, 200, /* US  */ 228},   // 37-43
                new[] {209, 667, 357, 430},             // 49-55
            };
            us = GetPelagoArea(speciesUU, minLevels);
            speciesUU[3][3] = 309; // Houndour -> Electrike
            um = GetPelagoArea(speciesUU, minLevels);
        }

        private static EncounterArea7[] GetPelagoArea(int[][] species, int[] min)
        {
            // Species that appear at a lower level than the current table show up too.
            var area = new EncounterArea7
            {
                Location = 30016,
                Slots = species.SelectMany((_, i) =>
                    species.Take(1 + i).SelectMany(z => // grab current row & above
                    z.Select(s => new EncounterSlot7 // get slot data for each species
                    {
                        Species = s,
                        LevelMin = min[i],
                        LevelMax = min[i] + 6
                    }
                    ))).ToArray(),
            };
            return new[] { area };
        }
    }
}