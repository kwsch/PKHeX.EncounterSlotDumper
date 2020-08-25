using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper
{
    public static class Dumper3
    {
        public static void DumpGen3()
        {
            var r = Resources.encounter_r;
            var s = Resources.encounter_s;
            var e = Resources.encounter_e;

            var f = Resources.encounter_fr;
            var l = Resources.encounter_lg;

            var ru = EncounterArea3.GetArray3(BinLinker.Unpack(r, "ru"));
            var sa = EncounterArea3.GetArray3(BinLinker.Unpack(s, "sa"));
            var em = EncounterArea3.GetArray3(BinLinker.Unpack(e, "em"));
            EncounterUtil.MarkEncountersStaticMagnetPull<EncounterSlot3>(em, PersonalTable.E);

            var fr = EncounterArea3.GetArray3(BinLinker.Unpack(f, "fr"));
            var lg = EncounterArea3.GetArray3(BinLinker.Unpack(l, "lg"));

            var rd = ru.Concat(FishFeebas).OrderBy(z => z.Location).ThenBy(z => z.Type);
            var sd = sa.Concat(FishFeebas).OrderBy(z => z.Location).ThenBy(z => z.Type);
            var ed = em.Concat(FishFeebas).OrderBy(z => z.Location).ThenBy(z => z.Type);

            var fd = fr.Concat(SlotsFRLGUnown).OrderBy(z => z.Location).ThenBy(z => z.Type);
            var ld = lg.Concat(SlotsFRLGUnown).OrderBy(z => z.Location).ThenBy(z => z.Type);

            Write(rd, "encounter_r.pkl", "ru");
            Write(sd, "encounter_s.pkl", "sa");
            Write(ed, "encounter_e.pkl", "em");
            Write(fd, "encounter_fr.pkl", "fr");
            Write(ld, "encounter_lg.pkl", "lg");

            WriteSwarm(SlotsRSEAlt, "encounter_rse_swarm.pkl", "rs");
        }

        public static void Write(IEnumerable<EncounterArea3> area, string name, string ident = "g3")
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

        public static byte[] Write(EncounterArea3 area)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(area.Location);
            bw.Write((byte)area.Type);
            bw.Write((byte)area.Rate);

            foreach (var slot in area.Slots.Cast<EncounterSlot3>())
                WriteSlot(bw, slot);

            return ms.ToArray();
        }

        private static void WriteSlot(BinaryWriter bw, EncounterSlot3 slot)
        {
            bw.Write((ushort)slot.Species);
            bw.Write((byte)slot.Form);
            bw.Write((byte)slot.SlotNumber);
            bw.Write((byte)slot.LevelMin);
            bw.Write((byte)slot.LevelMax);
            bw.Write((byte)slot.MagnetPullIndex);
            bw.Write((byte)slot.MagnetPullCount);
            bw.Write((byte)slot.StaticIndex);
            bw.Write((byte)slot.StaticCount);
        }

        public static void WriteSwarm(IEnumerable<EncounterArea3> area, string name, string ident = "g3")
        {
            var serialized = area.Select(WriteSwarm).ToArray();
            var packed = BinLinker.Pack(serialized, ident);
            File.WriteAllBytes(name, packed);
        }

        public static byte[] WriteSwarm(EncounterArea3 area)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(area.Location);
            bw.Write((byte)area.Type);
            bw.Write((byte)area.Rate);

            foreach (var slot in area.Slots.Cast<EncounterSlot3Swarm>())
                WriteSlotSwarm(bw, slot);

            return ms.ToArray();
        }

        private static void WriteSlotSwarm(BinaryWriter bw, EncounterSlot3Swarm slot)
        {
            bw.Write((ushort)slot.Species);
            bw.Write((byte)slot.Form);
            bw.Write((byte)slot.SlotNumber);
            bw.Write((byte)slot.LevelMin);
            bw.Write((byte)slot.LevelMax);
            foreach (var move in slot.Moves)
                bw.Write((ushort)move);

            // Magnet/Static are all 0's; don't bother.
        }

        private static readonly int[] MoveSwarmSurskit = { 145, 098, 000, 000 }; /* Bubble, Quick Attack */
        private static readonly int[] MoveSwarmSeedot = { 117, 106, 073, 000 };  /* Bide, Harden, Leech Seed */
        private static readonly int[] MoveSwarmNuzleaf = { 106, 074, 267, 073 }; /* Harden, Growth, Nature Power, Leech Seed */
        private static readonly int[] MoveSwarmSeedotF = { 202, 218, 076, 073 }; /* Giga Drain, Frustration, Solar Beam, Leech Seed */
        private static readonly int[] MoveSwarmSkittyRS = { 045, 033, 000, 000 }; /* Growl, Tackle */
        private static readonly int[] MoveSwarmSkittyE = { 045, 033, 039, 213 }; /* Growl, Tackle, Tail Whip, Attract */

        private static readonly EncounterArea3[] SlotsRSEAlt =
        {
            // Swarm can be passed from R/S<->E via mixing records
            // Encounter Percent is a 50% call
            new EncounterArea3 {
                Location = 17, // Route 102
                Type = SlotType.Swarm | SlotType.Grass,
                Rate = 20,
                Slots = new EncounterSlot[]
                {
                    new EncounterSlot3Swarm(MoveSwarmSurskit) { Species = 283, LevelMin = 03, LevelMax = 03 },
                    new EncounterSlot3Swarm(MoveSwarmSeedot) { Species = 273, LevelMin = 03, LevelMax = 03 },
                },},
            new EncounterArea3 {
                Location = 29, // Route 114
                Type = SlotType.Swarm | SlotType.Grass,
                Rate = 20,
                Slots = new EncounterSlot[]
                {
                    new EncounterSlot3Swarm(MoveSwarmSurskit) { Species = 283, LevelMin = 15, LevelMax = 15 },
                    new EncounterSlot3Swarm(MoveSwarmNuzleaf) { Species = 274, LevelMin = 15, LevelMax = 15 },
                },},
            new EncounterArea3 {
                Location = 31, // Route 116
                Type = SlotType.Swarm | SlotType.Grass,
                Rate = 20,
                Slots = new EncounterSlot[]
                {
                    new EncounterSlot3Swarm(MoveSwarmSkittyRS) { Species = 300, LevelMin = 15, LevelMax = 15 },
                    new EncounterSlot3Swarm(MoveSwarmSkittyE) { Species = 300, LevelMin = 08, LevelMax = 08 },
                },},
            new EncounterArea3 {
                Location = 32, // Route 117
                Type = SlotType.Swarm | SlotType.Grass,
                Rate = 20,
                Slots = new EncounterSlot[]
                {
                    new EncounterSlot3Swarm(MoveSwarmSurskit) { Species = 283, LevelMin = 15, LevelMax = 15 },
                    new EncounterSlot3Swarm(MoveSwarmNuzleaf) { Species = 273, LevelMin = 13, LevelMax = 13 }, // Has same moves as Nuzleaf
                },},
            new EncounterArea3 {
                Location = 35, // Route 120
                Type = SlotType.Swarm | SlotType.Grass,
                Rate = 20,
                Slots = new EncounterSlot[]
                {
                    new EncounterSlot3Swarm(MoveSwarmSurskit) { Species = 283, LevelMin = 28, LevelMax = 28},
                    new EncounterSlot3Swarm(MoveSwarmSeedotF) { Species = 273, LevelMin = 25, LevelMax = 25},
                },},
        };

        public static EncounterArea3[] FishFeebas =
        {
            // Feebas fishing spot
            new EncounterArea3
            {
                Location = 34, // Route 119
                Type = SlotType.Swarm | SlotType.Super_Rod,
                Slots = new[]
                {
                    new EncounterSlot3 {Species = 349, LevelMin = 20, LevelMax = 25} // Feebas with any Rod (50%)
                },
            },
        };

        private static readonly EncounterArea3[] SlotsFRLGUnown =
        {
            GetUnownArea(188, new[] { 00,00,00,00,00,00,00,00,00,00,00,27 }), // 188 = Monean Chamber
            GetUnownArea(189, new[] { 02,02,02,03,03,03,07,07,07,20,20,14 }), // 189 = Liptoo Chamber
            GetUnownArea(190, new[] { 13,13,13,13,18,18,18,18,08,08,04,04 }), // 190 = Weepth Chamber
            GetUnownArea(191, new[] { 15,15,11,11,09,09,17,17,17,16,16,16 }), // 191 = Dilford Chamber
            GetUnownArea(192, new[] { 24,24,19,19,06,06,06,05,05,05,10,10 }), // 192 = Scufib Chamber
            GetUnownArea(193, new[] { 21,21,21,22,22,22,23,23,12,12,01,01 }), // 193 = Rixy Chamber
            GetUnownArea(194, new[] { 25,25,25,25,25,25,25,25,25,25,25,26 }), // 194 = Viapois Chamber
        };

        private static EncounterArea3 GetUnownArea(short location, IReadOnlyList<int> SlotForms)
        {
            return new EncounterArea3
            {
                Location = location,
                Type = SlotType.Grass,
                Slots = SlotForms.Select((_, i) => new EncounterSlot3
                {
                    Species = 201,
                    LevelMin = 25,
                    LevelMax = 25,
                    SlotNumber = i,
                    Form = SlotForms[i]
                }).ToArray()
            };
        }
    }
}