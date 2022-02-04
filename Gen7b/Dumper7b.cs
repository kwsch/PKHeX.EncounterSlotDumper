using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PKHeX.EncounterSlotDumper
{
    public static class Dumper7b
    {
        private static byte[][] Get(byte[] data, string ident) => BinLinker.Unpack(data, ident);

        public static void DumpGen7b()
        {
            EncounterArea7b[] SlotsGP = EncounterArea7b.GetAreas(Get(Properties.Resources.encounter_gp, "gg"));
            EncounterArea7b[] SlotsGE = EncounterArea7b.GetAreas(Get(Properties.Resources.encounter_ge, "gg"));
            ManuallyAddRareSpawns(SlotsGP);
            ManuallyAddRareSpawns(SlotsGE);

            var gp = SlotsGP.Select(z => z.WriteSlots()).ToArray();
            var ge = SlotsGE.Select(z => z.WriteSlots()).ToArray();
            File.WriteAllBytes("encounter_gp.pkl", BinLinker.Pack(gp, "gg"));
            File.WriteAllBytes("encounter_ge.pkl", BinLinker.Pack(ge, "gg"));
        }

        private class RareSpawn
        {
            public readonly int Species;
            public readonly byte[] Locations;

            protected internal RareSpawn(int species, params byte[] locations)
            {
                Species = species;
                Locations = locations;
            }
        }

        private static readonly byte[] Sky = { 003, 004, 005, 006, 009, 010, 011, 012, 013, 014, 015, 016, 017, 018, 019, 020, 021, 022, 023, 024, 025, 026, 027 };

        private static readonly RareSpawn[] Rare =
        {
            // Normal
            new(001, 039),
            new(004, 005, 006, 041),
            new(007, 026, 027, 044),
            new(106, 045),
            new(107, 045),
            new(113, 007, 008, 010, 011, 012, 013, 014, 015, 016, 017, 018, 019, 020, 023, 025, 040, 042, 043, 045, 047, 051),
            new(137, 009),
            new(143, 046),

            // Water
            new(131, 021, 022),

            // Fly
            new(006, Sky),
            new(144, Sky),
            new(145, Sky),
            new(146, Sky),
            new(149, Sky),
        };

        private static void ManuallyAddRareSpawns(IEnumerable<EncounterArea7b> areas)
        {
            foreach (var table in areas)
            {
                var loc = table.Location;
                var species = Rare.Where(z => z.Locations.Contains((byte)loc)).Select(z => z.Species).ToArray();
                if (species.Length == 0)
                    continue;

                var slots = table.Slots;
                var extra = species
                    .Select(z => new EncounterSlot7b(z, GetMinLevel(z, slots, loc), GetMaxLevel(z, slots, loc))).ToArray();

                int count = slots.Length;
                Array.Resize(ref slots, count + extra.Length);
                extra.CopyTo(slots, count);
                table.Slots = slots;
            }
        }

        private static int GetMaxLevel(int z, EncounterSlot[] slots, int loc)
        {
            if (loc == 22 && z == 131) // Route 20 Lapras
                return 44; // Slot tables were already merged. Just merge the resulting Lapras'es.
            return (z is 006 or >= 144) ? 56 : slots[0].LevelMax;
        }

        private static int GetMinLevel(int z, EncounterSlot[] slots, int loc)
        {
            if (loc == 22 && z == 131) // Route 20 Lapras
                return 37; // Slot tables were already merged. Just merge the resulting Lapras'es.
            return (z is 006 or >= 144) ? 03 : slots[0].LevelMin;
        }
    }
    public sealed class EncounterArea7b : EncounterArea
    {
        public static EncounterArea7b[] GetAreas(byte[][] input)
        {
            var result = new EncounterArea7b[input.Length];
            for (int i = 0; i < input.Length; i++)
                result[i] = new EncounterArea7b(input[i]);
            return result;
        }

        private EncounterArea7b(byte[] data)
        {
            Location = BitConverter.ToInt16(data, 0);
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
                int species = SpecForm & 0x3FF;
                int min = data[offset + 2];
                int max = data[offset + 3];
                slots[i] = new EncounterSlot7b(species, min, max);
            }
            return slots;
        }

        public byte[] WriteSlots()
        {
            const int size = 4;
            var data = new byte[2 + (size * Slots.Length)];
            BitConverter.GetBytes((short) Location).CopyTo(data, 0);

            for (int i = 0; i < Slots.Length; i++)
            {
                var slot = Slots[i];
                int offset = 2 + (size * i);
                ushort SpecForm = (ushort)(slot.Species | (slot.Form << 11));
                Debug.Assert(SpecForm < 0x3FF);
                BitConverter.GetBytes(SpecForm).CopyTo(data, offset);
                data[offset + 2] = (byte)slot.LevelMin;
                data[offset + 3] = (byte)slot.LevelMax;
            }
            return data;
        }
    }

    public sealed class EncounterSlot7b : EncounterSlot
    {
        public EncounterSlot7b(int species, int min, int max)
        {
            Species = species;
            Form = 0;
            LevelMin = min;
            LevelMax = max;
        }
    }
}
