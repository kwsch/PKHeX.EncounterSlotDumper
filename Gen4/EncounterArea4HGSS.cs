using System;
using System.Collections.Generic;
using System.Linq;

namespace PKHeX.EncounterSlotDumper
{
    public sealed class EncounterArea4HGSS : EncounterArea4
    {
        /// <summary>
        /// Gets the encounter areas with <see cref="EncounterSlot"/> information from Generation 4 Heart Gold and Soul Silver data.
        /// </summary>
        /// <param name="entries">Raw data, one byte array per encounter area</param>
        /// <returns>Array of encounter areas.</returns>
        public static EncounterArea4HGSS[] GetArray4HGSS(byte[][] entries)
        {
            return entries.SelectMany(GetArea4HGSS).Where(Area => Area.Slots.Length != 0).ToArray();
        }

        /// <summary>
        /// Gets the encounter areas with <see cref="EncounterSlot"/> information from Generation 4 Heart Gold and Soul Silver Headbutt tree data.
        /// </summary>
        /// <param name="entries">Raw data, one byte array per encounter area</param>
        /// <returns>Array of encounter areas.</returns>
        public static EncounterArea4HGSS[] GetArray4HGSS_Headbutt(byte[][] entries)
        {
            return entries.SelectMany(GetArea4HeadbuttHGSS).Where(Area => Area.Slots.Length != 0).ToArray();
        }

        private static EncounterSlot4[] GetSlots4GrassHGSS(byte[] data, int ofs, int numslots)
        {
            var slots = new EncounterSlot4[numslots * 3];
            // First 36 slots are morning, day and night grass slots
            // The order is 12 level values, 12 morning species, 12 day species and 12 night species
            for (int i = 0; i < numslots; i++)
            {
                int level = data[ofs + i];
                int species = BitConverter.ToUInt16(data, ofs + numslots + (i * 2));
                slots[i] = new EncounterSlot4
                {
                    LevelMin = level,
                    LevelMax = level,
                    Species = species,
                    SlotNumber = i,
                };
                slots[numslots + i] = (EncounterSlot4)slots[i].Clone();
                slots[numslots + i].Species = BitConverter.ToUInt16(data, ofs + (numslots * 3) + (i * 2));
                slots[(numslots * 2) + i] = (EncounterSlot4)slots[i].Clone();
                slots[(numslots * 2) + i].Species = BitConverter.ToUInt16(data, ofs + (numslots * 5) + (i * 2));
            }

            return slots;
        }

        private static IEnumerable<EncounterSlot4> GetSlots4WaterFishingHGSS(EncounterArea4 area, byte[] data, int ofs, int numslots, SlotType t)
        {
            var slots = new List<EncounterSlot4>();
            for (int i = 0; i < numslots; i++)
            {
                // min, max, [16bit species]
                int Species = BitConverter.ToInt16(data, ofs + 2 + (i * 4));
                if (t == SlotType.Rock_Smash && Species <= 0)
                    continue;
                // Fishing and surf Slots without a species ID are added too; these are needed for the swarm encounters.
                // These empty slots will will be deleted after we add swarm slots.

                slots.Add(new EncounterSlot4
                {
                    LevelMin = data[ofs + 0 + (i * 4)],
                    LevelMax = data[ofs + 1 + (i * 4)],
                    Species = Species,
                    SlotNumber = i,
                });
            }

            area.Slots = slots.ToArray();
            EncounterUtil.MarkEncountersStaticMagnetPull<EncounterSlot4>(new [] {area}, PersonalTable.HGSS);
            return slots;
        }

        private static IEnumerable<EncounterArea4HGSS> GetArea4HGSS(byte[] data)
        {
            var Slots = new List<EncounterSlot4>();
            short location = BitConverter.ToInt16(data, 0x00);

            var GrassRate = data[0x02];
            var SurfRate = data[0x03];
            var RockSmashRate = data[0x04];
            var OldRate = data[0x05];
            var GoodRate = data[0x06];
            var SuperRate = data[0x07];
            // 2 bytes padding

            if (GrassRate > 0)
            {
                // First 36 slots are morning, day and night grass slots
                // The order is 12 level values, 12 morning species, 12 day species and 12 night species
                var GrassSlots = GetSlots4GrassHGSS(data, 0x0A, 12);
                //Grass slots with species = 0 are added too, it is needed for the swarm encounters, it will be deleted after swarms are added

                // Hoenn Sound and Sinnoh Sound replace slots 4 and 5
                var hoenn = GetSlots4GrassSlotReplace(data, 0x5E, 2, GrassSlots, Legal.Slot4_Sound); // Hoenn
                var sinnoh = GetSlots4GrassSlotReplace(data, 0x62, 2, GrassSlots, Legal.Slot4_Sound); // Sinnoh

                Slots.AddRange(GrassSlots);
                Slots.AddRange(hoenn);
                Slots.AddRange(sinnoh);

                // Static / Magnet Pull
                var grass1 = GrassSlots.Take(12).ToList();
                var grass2 = GrassSlots.Skip(12).Take(12).ToList();
                var grass3 = GrassSlots.Skip(24).ToList();
                // Swarm slots do not displace electric/steel types, with exception of SoulSilver Mawile (which doesn't displace) -- handle separately

                foreach (var time in new[] { grass1, grass2, grass3 })
                {
                    // non radio
                    var regular = time.Where(z => !Legal.Slot4_Sound.Contains(z.SlotNumber)).ToList(); // every other slot is in the product
                    var radio = new List<List<EncounterSlot4>> { time.Where(z => Legal.Slot4_Sound.Contains(z.SlotNumber)).ToList() };
                    if (hoenn.Count > 0)
                        radio.Add(hoenn);
                    if (sinnoh.Count > 0)
                        radio.Add(sinnoh);

                    var extra = new List<EncounterSlot4>();
                    foreach (var t in radio)
                        MarkStaticMagnetPermute(regular.Concat(t), extra);
                    Slots.AddRange(extra);
                }

                yield return new EncounterArea4HGSS {Location = location, Rate = GrassRate, Type = SlotType.Grass, Slots = Slots.ToArray()};
            }

            if (SurfRate > 0)
            {
                var area = new EncounterArea4HGSS { Location = location, Type = SlotType.Surf, Rate = SurfRate };
                GetSlots4WaterFishingHGSS(area, data, 0x66, 5, SlotType.Surf);
                yield return area;
            }

            if (RockSmashRate > 0)
            {
                var area = new EncounterArea4HGSS { Location = location, Type = SlotType.Rock_Smash, Rate = RockSmashRate };
                GetSlots4WaterFishingHGSS(area, data, 0x7A, 2, SlotType.Rock_Smash);
                yield return area;
            }

            var s_grass = (Species)BitConverter.ToUInt16(data, 0xBE + 0); // Grass Swarm
            var s_surf = (Species)BitConverter.ToUInt16(data, 0xBE + 2); // Surf Swarm
            var s_good = (Species)BitConverter.ToUInt16(data, 0xBE + 4); // Good Night
            var s_super = (Species)BitConverter.ToUInt16(data, 0xBE + 6); // Super Night

            if (OldRate > 0)
            {
                var area = new EncounterArea4HGSS { Location = location, Type = SlotType.Old_Rod, Rate = OldRate };
                GetSlots4WaterFishingHGSS(area, data, 0x82, 5, SlotType.Old_Rod);

                if (s_surf != 0 || s_grass != 0)
                {
                    //throw new Exception(); // no night fish for old rod, sanity check
                }

                yield return area;
            }

            if (GoodRate > 0)
            {
                var area = new EncounterArea4HGSS { Location = location, Type = SlotType.Good_Rod, Rate = GoodRate };
                GetSlots4WaterFishingHGSS(area, data, 0x96, 5, SlotType.Good_Rod);

                if (s_good == Species.Staryu || location == 219 && s_good == Species.Gyarados) // Staryu @ Location = 182, 127, 130, 132, 167, 188, 210
                {
                    var exist = area.Slots[1];
                    var slots = area.Slots.ToList();
                    slots.Insert(2, new EncounterSlot4 {Species = (int) s_good, LevelMin = exist.LevelMin, LevelMax = exist.LevelMax, SlotNumber = 1});
                    area.Slots = slots.ToArray();
                }

                yield return area;
            }

            if (SuperRate > 0)
            {
                var area = new EncounterArea4HGSS { Location = location, Type = SlotType.Super_Rod, Rate = SuperRate };
                GetSlots4WaterFishingHGSS(area, data, 0xAA, 5, SlotType.Super_Rod);

                if (s_good == Species.Staryu || location == 219 && s_good == Species.Gyarados) // Staryu @ Location = 182, 127, 130, 132, 167, 188, 210
                {
                    var exist = area.Slots[1];
                    var slots = area.Slots.ToList();
                    slots.Insert(2, new EncounterSlot4 { Species = (int)s_good, LevelMin = exist.LevelMin, LevelMax = exist.LevelMax, SlotNumber = 1 });
                    area.Slots = slots.ToArray();
                }
                yield return area;
            }
        }

        private static IEnumerable<EncounterArea4HGSS> GetArea4HeadbuttHGSS(byte[] data)
        {
            if (data.Length < 78)
                yield break;

            //2 byte location ID
            short location = BitConverter.ToInt16(data, 0);
            //4 bytes padding
            var Slots = new List<EncounterSlot>();

            // 00-11 Normal trees
            var area = new EncounterArea4HGSS { Location = location, Type = SlotType.Headbutt | SlotType.Special };
            for (int i = 0; i < 12; i++)
            {
                int Species = BitConverter.ToInt16(data, 6 + (i * 4));
                if (Species <= 0)
                    continue;
                Slots.Add(new EncounterSlot4
                {
                    Species = Species,
                    LevelMin = data[8 + (i * 4)],
                    LevelMax = data[9 + (i * 4)],
                });
            }

            if (Slots.Count > 0)
            {
                area.Slots = Slots.ToArray();
                yield return area;
            }

            // 12-17 Special trees
            area = new EncounterArea4HGSS{Location = location, Type = SlotType.Headbutt | SlotType.Special};
            Slots.Clear();
            for (int i = 12; i < 18; i++)
            {
                int Species = BitConverter.ToInt16(data, 6 + (i * 4));
                if (Species <= 0)
                    continue;
                Slots.Add(new EncounterSlot4
                {
                    Species = Species,
                    LevelMin = data[8 + (i * 4)],
                    LevelMax = data[9 + (i * 4)],
                });
            }

            if (Slots.Count > 0)
            {
                area.Slots = Slots.ToArray();
                yield return area;
            }
        }
    }
}