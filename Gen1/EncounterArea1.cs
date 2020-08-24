using System;
using System.Collections.Generic;

namespace PKHeX.EncounterSlotDumper
{
    public sealed class EncounterArea1 : EncounterArea
    {
        /// <summary>
        /// Wild Encounter activity rate
        /// </summary>
        public int Rate { get; set; }

        private static EncounterSlot1[] ReadSlots1FishingYellow(byte[] data, ref int ofs, int count)
        {
            // Convert byte to actual number
            byte[] levels = {0xFF, 0x15, 0x67, 0x1D, 0x3B, 0x5C, 0x72, 0x16, 0x71, 0x18, 0x00, 0x6D, 0x80,};
            byte[] g1DexIDs = {0x47, 0x6E, 0x18, 0x9B, 0x17, 0x4E, 0x8A, 0x5C, 0x5D, 0x9D, 0x9E, 0x1B, 0x85, 0x16, 0x58, 0x59,};
            int[] speciesIDs = {060, 061, 072, 073, 090, 098, 099, 116, 117, 118, 119, 120, 129, 130, 147, 148,};

            var slots = new EncounterSlot1[count];
            for (int slot = 0; slot < count; slot++)
            {
                int species = speciesIDs[Array.IndexOf(g1DexIDs, data[ofs++])];
                int lvl = Array.IndexOf(levels, data[ofs++]) * 5;
                slots[slot] = new EncounterSlot1(species, lvl, lvl, slot);
            }

            return slots;
        }

        /// <summary>
        /// Gets the encounter areas with <see cref="EncounterSlot"/> information from Generation 1 Grass/Water data.
        /// </summary>
        /// <param name="data">Input raw data.</param>
        /// <param name="count">Count of areas in the binary.</param>
        /// <returns>Array of encounter areas.</returns>
        public static EncounterArea1[] GetArray1GrassWater(byte[] data, int count)
        {
            var areas = new List<EncounterArea1>(count);
            for (int i = 0; i < count; i++)
            {
                int ptr = BitConverter.ToInt16(data, i * 2);
                var g = new EncounterArea1
                {
                    Type = SlotType.Grass,
                    Location = (short)i,
                };

                var gslots = GetSlots1GrassWater(data, g, ref ptr);
                if (gslots.Length > 0)
                {
                    areas.Add(g);
                    g.Slots = gslots;
                }

                var w = new EncounterArea1
                {
                    Type = SlotType.Surf,
                    Location = (short)i,
                };
                var wslots = GetSlots1GrassWater(data, w, ref ptr);
                if (wslots.Length > 0)
                {
                    areas.Add(w);
                    w.Slots = wslots;
                }
            }

            return areas.ToArray();
        }

        /// <summary>
        /// Gets the encounter areas with <see cref="EncounterSlot"/> information from Pokémon Yellow (Generation 1) Fishing data.
        /// </summary>
        /// <param name="data">Input raw data.</param>
        /// <returns>Array of encounter areas.</returns>
        public static EncounterArea1[] GetArray1FishingYellow(byte[] data)
        {
            const int size = 9;
            int count = data.Length / size;
            EncounterArea1[] areas = new EncounterArea1[count];
            for (int i = 0; i < count; i++)
            {
                int ofs = (i * size) + 1;
                areas[i] = new EncounterArea1
                {
                    Location = data[(i * size) + 0],
                    Type = SlotType.Super_Rod,
                    Slots = ReadSlots1FishingYellow(data, ref ofs, 4)
                };
            }

            return areas;
        }

        /// <summary>
        /// Gets the encounter areas with <see cref="EncounterSlot"/> information from Generation 1 Fishing data.
        /// </summary>
        /// <param name="data">Input raw data.</param>
        /// <param name="count">Count of areas in the binary.</param>
        /// <returns>Array of encounter areas.</returns>
        public static EncounterArea1[] GetArray1Fishing(byte[] data, int count)
        {
            var areas = new EncounterArea1[count];
            for (int i = 0; i < areas.Length; i++)
            {
                int loc = data[(i * 3) + 0];
                int ptr = BitConverter.ToInt16(data, (i * 3) + 1);
                areas[i] = new EncounterArea1
                {
                    Location = (short)loc,
                    Type = SlotType.Super_Rod,
                    Slots = GetSlots1Fishing(data, ptr)
                };
            }

            return areas;
        }

        private static EncounterSlot1[] GetSlots1GrassWater(byte[] data, EncounterArea1 a, ref int ofs)
        {
            int rate = data[ofs++];
            a.Rate = rate;
            return rate == 0 ? Array.Empty<EncounterSlot1>() : EncounterSlot1.ReadSlots(data, ref ofs, 10, SlotType.Grass);
        }

        private static EncounterSlot1[] GetSlots1Fishing(byte[] data, int ofs)
        {
            int count = data[ofs++];
            return EncounterSlot1.ReadSlots(data, ref ofs, count, SlotType.Super_Rod);
        }
    }
}