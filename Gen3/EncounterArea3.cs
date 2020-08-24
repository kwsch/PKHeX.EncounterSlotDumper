using System;
using System.Collections.Generic;

namespace PKHeX.EncounterSlotDumper
{
    public sealed class EncounterArea3 : EncounterArea
    {
        public int Rate { get; set; }

        private static void GetSlots3(byte[] data, ref int ofs, int numslots, List<EncounterArea3> areas, short location, SlotType t)
        {
            int rate = data[ofs];
            //1 byte padding
            if (rate > 0)
                ReadInSlots(data, ofs, numslots, areas, location, t, rate);
            ofs += 2 + (numslots * 4);
        }

        private static void ReadInSlots(byte[] data, int ofs, int numslots, List<EncounterArea3> areas, short location, SlotType t, int rate)
        {
            var area = new EncounterArea3 {Location = location, Type = t, Rate = rate};
            var slots = new List<EncounterSlot3>();
            for (int i = 0; i < numslots; i++)
            {
                int o = ofs + (i * 4);
                int species = BitConverter.ToInt16(data, o + 4);
                if (species <= 0)
                    continue;

                slots.Add(new EncounterSlot3
                {
                    LevelMin = data[o + 2],
                    LevelMax = data[o + 3],
                    Species = species,
                    SlotNumber = i,
                });
            }

            area.Slots = slots.ToArray();
            areas.Add(area);
        }

        private static void GetSlots3Fishing(byte[] data, ref int ofs, int numslots, List<EncounterArea3> areas, short location)
        {
            int Ratio = data[ofs];
            //1 byte padding
            if (Ratio > 0)
                ReadFishingSlots(data, ofs, numslots, areas, location);
            ofs += 2 + (numslots * 4);
        }

        private static void ReadFishingSlots(byte[] data, int ofs, int numslots, List<EncounterArea3> areas, short location)
        {
            var o = new List<EncounterSlot>();
            var g = new List<EncounterSlot>();
            var s = new List<EncounterSlot>();
            for (int i = 0; i < numslots; i++)
            {
                int species = BitConverter.ToInt16(data, ofs + 4 + (i * 4));
                if (species <= 0)
                    continue;

                var slot = new EncounterSlot3
                {
                    LevelMin = data[ofs + 2 + (i * 4)],
                    LevelMax = data[ofs + 3 + (i * 4)],
                    Species = species,
                };

                if (i < 2)
                {
                    o.Add(slot);
                    slot.SlotNumber = i; // 0,1
                }
                else if (i < 5)
                {
                    g.Add(slot);
                    slot.SlotNumber = i - 2; // 0,1,2
                }
                else
                {
                    s.Add(slot);
                    slot.SlotNumber = i - 5; // 0,1,2,3,4
                }
            }

            var oa = new EncounterArea3 { Location = location, Type = SlotType.Old_Rod, Slots = o.ToArray() };
            var ga = new EncounterArea3 { Location = location, Type = SlotType.Good_Rod, Slots = g.ToArray() };
            var sa = new EncounterArea3 { Location = location, Type = SlotType.Super_Rod, Slots = s.ToArray() };
            areas.Add(oa);
            areas.Add(ga);
            areas.Add(sa);
        }

        private static void GetArea3(byte[] data, List<EncounterArea3> areas)
        {
            short location = data[0];
            var HaveGrassSlots = data[1] == 1;
            var HaveSurfSlots = data[2] == 1;
            var HaveRockSmashSlots = data[3] == 1;
            var HaveFishingSlots = data[4] == 1;

            int offset = 5;
            if (HaveGrassSlots)
                GetSlots3(data, ref offset, 12, areas, location, SlotType.Grass);
            if (HaveSurfSlots)
                GetSlots3(data, ref offset, 5, areas, location, SlotType.Surf);
            if (HaveRockSmashSlots)
                GetSlots3(data, ref offset, 5, areas, location, SlotType.Rock_Smash);
            if (HaveFishingSlots)
                GetSlots3Fishing(data, ref offset, 10, areas, location);
        }

        /// <summary>
        /// Gets the encounter areas with <see cref="EncounterSlot"/> information from Generation 3 data.
        /// </summary>
        /// <param name="entries">Raw data, one byte array per encounter area</param>
        /// <returns>Array of encounter areas.</returns>
        public static EncounterArea3[] GetArray3(byte[][] entries)
        {
            var areas = new List<EncounterArea3>();
            foreach (var entry in entries)
                GetArea3(entry, areas);
            return areas.ToArray();
        }
    }
}