using PKHeX.EncounterSlotDumper.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PKHeX.EncounterSlotDumper
{
    public static class Dumper5
    {
        public static void DumpGen5()
        {
            var BSlots =  GetEncounterTables("51", Resources.encounter_b );
            var WSlots =  GetEncounterTables("51", Resources.encounter_w );
            var B2Slots = GetEncounterTables("52", Resources.encounter_b2);
            var W2Slots = GetEncounterTables("52", Resources.encounter_w2);

            static EncounterArea5[] GetEncounterTables(string ident, byte[] mini)
            {
                var data = BinLinker.Unpack(mini, ident);
                return EncounterArea32.GetArray<EncounterArea5, EncounterSlot5>(data);
            }

            MarkG5Slots(ref BSlots);
            MarkG5Slots(ref WSlots);
            MarkG5Slots(ref B2Slots);
            MarkG5Slots(ref W2Slots);
            MarkBWSwarmSlots(SlotsB_Swarm);
            MarkBWSwarmSlots(SlotsW_Swarm);
            MarkB2W2SwarmSlots(SlotsB2_Swarm);
            MarkB2W2SwarmSlots(SlotsW2_Swarm);
            MarkG5HiddenGrottoSlots(SlotsB2_HiddenGrotto);
            MarkG5HiddenGrottoSlots(SlotsW2_HiddenGrotto);

            var SlotsB = ArrayUtil.ConcatAll(BSlots, SlotsB_Swarm);
            var SlotsW = ArrayUtil.ConcatAll(WSlots, SlotsW_Swarm, WhiteForestSlot);
            var SlotsB2 = ArrayUtil.ConcatAll(B2Slots, SlotsB2_Swarm, SlotsB2_HiddenGrotto);
            var SlotsW2 = ArrayUtil.ConcatAll(W2Slots, SlotsW2_Swarm, SlotsW2_HiddenGrotto);

            Write(SlotsB,  "encounter_b.pkl" , "51");
            Write(SlotsW , "encounter_w.pkl" , "51");
            Write(SlotsB2, "encounter_b2.pkl", "52");
            Write(SlotsW2, "encounter_w2.pkl", "52");
        }

        private static void MarkBWSwarmSlots(EncounterArea5[] Areas)
        {
            foreach (var area in Areas)
            {
                area.Type = SlotType.Swarm;
                foreach (var s in area.Slots)
                {
                    s.LevelMin = 15;
                    s.LevelMax = 55;
                }
            }
        }

        private static void MarkB2W2SwarmSlots(EncounterArea5[] Areas)
        {
            foreach (var area in Areas)
            {
                area.Type = SlotType.Swarm;
                foreach (var s in area.Slots)
                {
                    s.LevelMin = 40;
                    s.LevelMax = 55;
                }
            }
        }

        private static void MarkG5HiddenGrottoSlots(EncounterArea5[] Areas)
        {
            Areas[0].Type = SlotType.HiddenGrotto;
        }

        private static void MarkG5Slots(ref EncounterArea5[] Areas)
        {
            List<EncounterArea5> areas = new List<EncounterArea5>();
            foreach (var area in Areas)
            {
                int ctr = 0;
                do
                {
                    areas.Add(new EncounterArea5 { Location = area.Location, Type = SlotType.Grass, Slots = area.Slots.Skip(ctr).Take(12).ToArray() }); // Single
                    ctr += 12;
                    areas.Add(new EncounterArea5 { Location = area.Location, Type = SlotType.Grass, Slots = area.Slots.Skip(ctr).Take(12).ToArray() }); // Double
                    ctr += 12;
                    areas.Add(new EncounterArea5 { Location = area.Location, Type = SlotType.Grass, Slots = area.Slots.Skip(ctr).Take(5).ToArray() }); // Shaking
                    ctr += 12;

                    areas.Add(new EncounterArea5 { Location = area.Location, Type = SlotType.Surf, Slots = area.Slots.Skip(ctr).Take(5).ToArray() }); // Surf
                    ctr += 5;
                    areas.Add(new EncounterArea5 { Location = area.Location, Type = SlotType.Surf, Slots = area.Slots.Skip(ctr).Take(5).ToArray() }); // Surf Spot
                    ctr += 5;
                    areas.Add(new EncounterArea5 { Location = area.Location, Type = SlotType.Super_Rod, Slots = area.Slots.Skip(ctr).Take(5).ToArray() }); // Fish
                    ctr += 5;
                    areas.Add(new EncounterArea5 { Location = area.Location, Type = SlotType.Super_Rod, Slots = area.Slots.Skip(ctr).Take(5).ToArray() }); // Fish Spot
                    ctr += 5;
                } while (ctr != area.Slots.Length);
                area.Slots = area.Slots.Where(slot => slot.Species != 0).ToArray();
            }

            Areas = areas.ToArray();
        }

        public static void Write(IEnumerable<EncounterArea5> area, string name, string ident = "g5")
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

        public static byte[] Write(EncounterArea5 area)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(area.Location);
            bw.Write((byte)area.Type);
            bw.Write((byte)0);

            foreach (var slot in area.Slots.Cast<EncounterSlot5>())
                WriteSlot(bw, slot);

            return ms.ToArray();
        }

        private static void WriteSlot(BinaryWriter bw, EncounterSlot5 slot)
        {
            bw.Write((ushort)(slot.Species | (slot.Form << 11)));
            bw.Write((byte)slot.LevelMin);
            bw.Write((byte)slot.LevelMax);
        }

        #region Alt Slots

        // White forest white version only

        private static readonly int[] WhiteForest_GrassSpecies =
        {
            016, 029, 032, 043, 063, 066, 069, 081, 092, 111,
            137, 175, 179, 187, 239, 240, 265, 270, 273, 280,
            287, 293, 298, 304, 328, 371, 396, 403, 406, 440,
        };

        private static readonly int[] WhiteForest_SurfSpecies =
        {
            194, 270, 283, 341,
        };

        private static readonly EncounterArea5[] WhiteForestSlot = EncounterArea.GetSimpleEncounterArea<EncounterArea5, EncounterSlot5>(WhiteForest_GrassSpecies, new[] { 5, 5 }, 51, SlotType.Grass).Concat(
            EncounterArea.GetSimpleEncounterArea<EncounterArea5, EncounterSlot5>(WhiteForest_SurfSpecies, new[] { 5, 5 }, 51, SlotType.Surf)).ToArray();

        private static readonly EncounterArea5[] SlotsBW_Swarm =
        {
            // Level Range and Slot Type will be marked later
            new EncounterArea5 { Location = 014, Slots = new[]{new EncounterSlot5 { Species = 083 }, }, }, // Farfetch'd @ Route 1
            new EncounterArea5 { Location = 015, Slots = new[]{new EncounterSlot5 { Species = 360 }, }, }, // Wynaut @ Route 2
            new EncounterArea5 { Location = 017, Slots = new[]{new EncounterSlot5 { Species = 449 }, }, }, // Hippopotas @ Route 4
            new EncounterArea5 { Location = 018, Slots = new[]{new EncounterSlot5 { Species = 235 }, }, }, // Smeargle @ Route 5
            new EncounterArea5 { Location = 020, Slots = new[]{new EncounterSlot5 { Species = 161 }, }, }, // Sentret @ Route 7
            new EncounterArea5 { Location = 021, Slots = new[]{new EncounterSlot5 { Species = 453 }, }, }, // Croagunk @ Route 8
            new EncounterArea5 { Location = 023, Slots = new[]{new EncounterSlot5 { Species = 236 }, }, }, // Tyrogue @ Route 10
            new EncounterArea5 { Location = 025, Slots = new[]{new EncounterSlot5 { Species = 084 }, }, }, // Doduo @ Route 12
            new EncounterArea5 { Location = 026, Slots = new[]{new EncounterSlot5 { Species = 353 }, }, }, // Shuppet @ Route 13
            new EncounterArea5 { Location = 027, Slots = new[]{new EncounterSlot5 { Species = 193 }, }, }, // Yanma @ Route 14
            new EncounterArea5 { Location = 028, Slots = new[]{new EncounterSlot5 { Species = 056 }, }, }, // Mankey @ Route 15
            new EncounterArea5 { Location = 029, Slots = new[]{new EncounterSlot5 { Species = 204 }, }, }, // Pineco @ Route 16
            new EncounterArea5 { Location = 031, Slots = new[]{new EncounterSlot5 { Species = 102 }, }, }, // Exeggcute @ Route 18
        };

        private static readonly EncounterArea5[] SlotsB_Swarm = SlotsBW_Swarm.Concat(new[] {
            new EncounterArea5 { Location = 016, Slots = new[]{new EncounterSlot5 { Species = 313 }, }, }, // Volbeat @ Route 3
            new EncounterArea5 { Location = 019, Slots = new[]{new EncounterSlot5 { Species = 311 }, }, }, // Plusle @ Route 6
            new EncounterArea5 { Location = 022, Slots = new[]{new EncounterSlot5 { Species = 228 }, }, }, // Houndour @ Route 9
            new EncounterArea5 { Location = 024, Slots = new[]{new EncounterSlot5 { Species = 285 }, }, }, // Shroomish @ Route 11
        }).ToArray();

        private static readonly EncounterArea5[] SlotsW_Swarm = SlotsBW_Swarm.Concat(new[] {
            new EncounterArea5 { Location = 016, Slots = new[]{new EncounterSlot5 { Species = 314 }, }, }, // Illumise @ Route 3
            new EncounterArea5 { Location = 019, Slots = new[]{new EncounterSlot5 { Species = 312 }, }, }, // Minun @ Route 6
            new EncounterArea5 { Location = 022, Slots = new[]{new EncounterSlot5 { Species = 261 }, }, }, // Poochyena @ Route 9
            new EncounterArea5 { Location = 024, Slots = new[]{new EncounterSlot5 { Species = 046 }, }, }, // Paras @ Route 11
        }).ToArray();

        private static readonly EncounterArea5[] SlotsB2W2_Swarm =
        {
            // Level Range and Slot Type will be marked later
            new EncounterArea5 { Location = 014, Slots = new[]{new EncounterSlot5 { Species = 083 }, }, }, // Farfetch'd @ Route 1
            new EncounterArea5 { Location = 018, Slots = new[]{new EncounterSlot5 { Species = 177 }, }, }, // Natu @ Route 5
            new EncounterArea5 { Location = 020, Slots = new[]{new EncounterSlot5 { Species = 162 }, }, }, // Furret @ Route 7
            new EncounterArea5 { Location = 021, Slots = new[]{new EncounterSlot5 { Species = 195 }, }, }, // Quagsire @ Route 8
            new EncounterArea5 { Location = 022, Slots = new[]{new EncounterSlot5 { Species = 317 }, }, }, // Swalot @ Route 9
            new EncounterArea5 { Location = 024, Slots = new[]{new EncounterSlot5 { Species = 284 }, }, }, // Masquerain @ Route 11
            new EncounterArea5 { Location = 025, Slots = new[]{new EncounterSlot5 { Species = 084 }, }, }, // Doduo @ Route 12
            new EncounterArea5 { Location = 026, Slots = new[]{new EncounterSlot5 { Species = 277 }, }, }, // Swellow @ Route 13
            new EncounterArea5 { Location = 028, Slots = new[]{new EncounterSlot5 { Species = 022 }, }, }, // Fearow @ Route 15
            new EncounterArea5 { Location = 029, Slots = new[]{new EncounterSlot5 { Species = 204 }, }, }, // Pineco @ Route 16
            new EncounterArea5 { Location = 031, Slots = new[]{new EncounterSlot5 { Species = 187 }, }, }, // Hoppip @ Route 18
            new EncounterArea5 { Location = 032, Slots = new[]{new EncounterSlot5 { Species = 097 }, }, }, // Hypno @ Dreamyard
            new EncounterArea5 { Location = 034, Slots = new[]{new EncounterSlot5 { Species = 450 }, }, }, // Hippowdon @ Desert Resort
            new EncounterArea5 { Location = 070, Slots = new[]{new EncounterSlot5 { Species = 079 }, }, }, // Slowpoke @ Abundant shrine
            new EncounterArea5 { Location = 132, Slots = new[]{new EncounterSlot5 { Species = 332 }, }, }, // Cacturne @ Reaversal Mountian
        };

        private static readonly EncounterArea5[] SlotsB2_Swarm = SlotsB2W2_Swarm.Concat(new[] {
            new EncounterArea5 { Location = 016, Slots = new[]{new EncounterSlot5 { Species = 313 }, }, }, // Volbeat @ Route 3
            new EncounterArea5 { Location = 019, Slots = new[]{new EncounterSlot5 { Species = 311 }, }, }, // Plusle @ Route 6
            new EncounterArea5 { Location = 125, Slots = new[]{new EncounterSlot5 { Species = 185 }, }, }, // Sudowoodo @ Route 20
            new EncounterArea5 { Location = 127, Slots = new[]{new EncounterSlot5 { Species = 168 }, }, }, // Ariados @ Route 22
        }).ToArray();

        private static readonly EncounterArea5[] SlotsW2_Swarm = SlotsB2W2_Swarm.Concat(new[] {
            new EncounterArea5 { Location = 016, Slots = new[]{new EncounterSlot5 { Species = 314 }, }, }, // Illumise @ Route 3
            new EncounterArea5 { Location = 019, Slots = new[]{new EncounterSlot5 { Species = 312 }, }, }, // Minun @ Route 6
            new EncounterArea5 { Location = 125, Slots = new[]{new EncounterSlot5 { Species = 122 }, }, }, // Mr. Mime @ Route 20
            new EncounterArea5 { Location = 127, Slots = new[]{new EncounterSlot5 { Species = 166 }, }, }, // Ledian @ Route 22
        }).ToArray();

        private static readonly EncounterSlot[] SlotsB2W2_HiddenGrottoEncounterSlots =
        {
            // reference http://bulbapedia.bulbagarden.net/wiki/Hidden_Grotto
            // Route 2
            new EncounterSlot5 { Species = 029, LevelMin = 55, LevelMax = 60, }, // Nidoran♀
            new EncounterSlot5 { Species = 032, LevelMin = 55, LevelMax = 60, }, // Nidoran♂
            new EncounterSlot5 { Species = 210, LevelMin = 55, LevelMax = 60, }, // Granbull
            new EncounterSlot5 { Species = 505, LevelMin = 55, LevelMax = 60, }, // Watchog

            // Route 3
            new EncounterSlot5 { Species = 310, LevelMin = 55, LevelMax = 60, }, // Manectric @ Dark Grass
            new EncounterSlot5 { Species = 417, LevelMin = 55, LevelMax = 60, }, // Pachirisu @ Dark Grass
            new EncounterSlot5 { Species = 523, LevelMin = 55, LevelMax = 60, }, // Zebstrika @ Dark Grass
            new EncounterSlot5 { Species = 048, LevelMin = 55, LevelMax = 60, }, // Venonat @ Pond
            new EncounterSlot5 { Species = 271, LevelMin = 55, LevelMax = 60, }, // Lombre @ Pond
            new EncounterSlot5 { Species = 400, LevelMin = 55, LevelMax = 60, }, // Bibarel @ Pond

            // Route 5
            new EncounterSlot5 { Species = 510, LevelMin = 20, LevelMax = 25, }, // Liepard
            new EncounterSlot5 { Species = 572, LevelMin = 20, LevelMax = 25, }, // Minccino
            new EncounterSlot5 { Species = 590, LevelMin = 20, LevelMax = 25, }, // Foongus

            // Route 6
            new EncounterSlot5 { Species = 206, LevelMin = 25, LevelMax = 30, }, // Dunsparce @ Near PKM Breeder
            new EncounterSlot5 { Species = 299, LevelMin = 25, LevelMax = 30, }, // Nosepass @ Mistralton Cave
            new EncounterSlot5 { Species = 527, LevelMin = 25, LevelMax = 30, }, // Woobat @ Both
            new EncounterSlot5 { Species = 590, LevelMin = 25, LevelMax = 30, }, // Foongus @ Both

            // Route 7
            new EncounterSlot5 { Species = 335, LevelMin = 30, LevelMax = 35, }, // Zangoose
            new EncounterSlot5 { Species = 336, LevelMin = 30, LevelMax = 35, }, // Seviper
            new EncounterSlot5 { Species = 505, LevelMin = 30, LevelMax = 35, }, // Watchog
            new EncounterSlot5 { Species = 613, LevelMin = 30, LevelMax = 35, }, // Cubchoo

            // Route 9
            new EncounterSlot5 { Species = 089, LevelMin = 35, LevelMax = 40, }, // Muk
            new EncounterSlot5 { Species = 510, LevelMin = 35, LevelMax = 40, }, // Liepard
            new EncounterSlot5 { Species = 569, LevelMin = 35, LevelMax = 40, }, // Garbodor
            new EncounterSlot5 { Species = 626, LevelMin = 35, LevelMax = 40, }, // Bouffalant

            // Route 13
            new EncounterSlot5 { Species = 114, LevelMin = 35, LevelMax = 40, }, // Tangela @ Gaint Chasm
            new EncounterSlot5 { Species = 363, LevelMin = 35, LevelMax = 40, }, // Spheal @ Stairs
            new EncounterSlot5 { Species = 425, LevelMin = 35, LevelMax = 40, }, // Drifloon @ Stairs
            new EncounterSlot5 { Species = 451, LevelMin = 35, LevelMax = 40, }, // Skorupi @ Gaint Chasm
            new EncounterSlot5 { Species = 590, LevelMin = 35, LevelMax = 40, }, // Foongus @ Both

            // Route 18
            new EncounterSlot5 { Species = 099, LevelMin = 55, LevelMax = 60, }, // Kingler
            new EncounterSlot5 { Species = 149, LevelMin = 55, LevelMax = 60, }, // Dragonite
            new EncounterSlot5 { Species = 222, LevelMin = 55, LevelMax = 60, }, // Corsola
            new EncounterSlot5 { Species = 441, LevelMin = 55, LevelMax = 60, }, // Chatot

            // Pinwheel Forest
            new EncounterSlot5 { Species = 061, LevelMin = 55, LevelMax = 60, }, // Poliwhirl @ Outer
            new EncounterSlot5 { Species = 198, LevelMin = 55, LevelMax = 60, }, // Murkrow @ Inner
            new EncounterSlot5 { Species = 286, LevelMin = 55, LevelMax = 60, }, // Breloom @ Inner
            new EncounterSlot5 { Species = 297, LevelMin = 55, LevelMax = 60, }, // Hariyama @ Outer
            new EncounterSlot5 { Species = 308, LevelMin = 55, LevelMax = 60, }, // Medicham @ Outer
            new EncounterSlot5 { Species = 371, LevelMin = 55, LevelMax = 60, }, // Bagon @ Outer
            new EncounterSlot5 { Species = 591, LevelMin = 55, LevelMax = 60, }, // Amoonguss @ Inner

            // Giant Chasm
            new EncounterSlot5 { Species = 035, LevelMin = 45, LevelMax = 50, }, // Clefairy
            new EncounterSlot5 { Species = 132, LevelMin = 45, LevelMax = 50, }, // Ditto
            new EncounterSlot5 { Species = 215, LevelMin = 45, LevelMax = 50, }, // Sneasel
            new EncounterSlot5 { Species = 375, LevelMin = 45, LevelMax = 50, }, // Metang

            // Abundant Shrine
            new EncounterSlot5 { Species = 037, LevelMin = 35, LevelMax = 40, }, // Vulpix @ Near Youngster
            new EncounterSlot5 { Species = 055, LevelMin = 35, LevelMax = 40, }, // Golduck @ Shrine
            new EncounterSlot5 { Species = 333, LevelMin = 35, LevelMax = 40, }, // Swablu @ Shrine
            new EncounterSlot5 { Species = 436, LevelMin = 35, LevelMax = 40, }, // Bronzor @ Near Youngster
            new EncounterSlot5 { Species = 591, LevelMin = 35, LevelMax = 40, }, // Amoonguss @ Both

            // Lostlorn Forest
            new EncounterSlot5 { Species = 127, LevelMin = 20, LevelMax = 25, }, // Pinsir
            new EncounterSlot5 { Species = 214, LevelMin = 20, LevelMax = 25, }, // Heracross
            new EncounterSlot5 { Species = 415, LevelMin = 20, LevelMax = 25, }, // Combee
            new EncounterSlot5 { Species = 542, LevelMin = 20, LevelMax = 25, }, // Leavanny

            // Route 22
            new EncounterSlot5 { Species = 279, LevelMin = 40, LevelMax = 45, }, // Pelipper
            new EncounterSlot5 { Species = 591, LevelMin = 40, LevelMax = 45, }, // Amoonguss
            new EncounterSlot5 { Species = 619, LevelMin = 40, LevelMax = 45, }, // Mienfoo

            // Route 23
            new EncounterSlot5 { Species = 055, LevelMin = 50, LevelMax = 55, }, // Golduck
            new EncounterSlot5 { Species = 207, LevelMin = 50, LevelMax = 55, }, // Gligar
            new EncounterSlot5 { Species = 335, LevelMin = 50, LevelMax = 55, }, // Zangoose
            new EncounterSlot5 { Species = 336, LevelMin = 50, LevelMax = 55, }, // Seviper
            new EncounterSlot5 { Species = 359, LevelMin = 50, LevelMax = 55, }, // Absol

            // Floccesy Ranch
            new EncounterSlot5 { Species = 183, LevelMin = 10, LevelMax = 15, }, // Marill
            new EncounterSlot5 { Species = 206, LevelMin = 10, LevelMax = 15, }, // Dunsparce
            new EncounterSlot5 { Species = 507, LevelMin = 10, LevelMax = 15, }, // Herdier

            // Funfest Missions
            // todo : check the level
            new EncounterSlot5 { Species = 133, LevelMin = 15, LevelMax = 60, }, // Eevee
            new EncounterSlot5 { Species = 134, LevelMin = 15, LevelMax = 60, }, // Vaporeon
            new EncounterSlot5 { Species = 135, LevelMin = 15, LevelMax = 60, }, // Jolteon
            new EncounterSlot5 { Species = 136, LevelMin = 15, LevelMax = 60, }, // Flareon
            new EncounterSlot5 { Species = 196, LevelMin = 15, LevelMax = 60, }, // Espeon
            new EncounterSlot5 { Species = 197, LevelMin = 15, LevelMax = 60, }, // Umbreon
            new EncounterSlot5 { Species = 470, LevelMin = 15, LevelMax = 60, }, // Leafeon
            new EncounterSlot5 { Species = 471, LevelMin = 15, LevelMax = 60, }, // Glaceon

            // Funfest Week 3
            // new EncounterSlot5 { Species = 060, LevelMin = 15, LevelMax = 60, }, // Poliwag
            new EncounterSlot5 { Species = 113, LevelMin = 15, LevelMax = 60, }, // Chansey
            new EncounterSlot5 { Species = 176, LevelMin = 15, LevelMax = 60, }, // Togetic
            new EncounterSlot5 { Species = 082, LevelMin = 15, LevelMax = 60, }, // Magneton
            new EncounterSlot5 { Species = 148, LevelMin = 15, LevelMax = 60, }, // Dragonair
            new EncounterSlot5 { Species = 372, LevelMin = 15, LevelMax = 60, }, // Shelgon                      
        };

        private static readonly EncounterArea5[] SlotsB2_HiddenGrotto =
        {
            new EncounterArea5
            {
                Location = 143, // Hidden Grotto
                Slots = SlotsB2W2_HiddenGrottoEncounterSlots.Concat(new[]{
                    new EncounterSlot5 { Species = 015, LevelMin = 55, LevelMax = 60 }, // Beedrill @ Pinwheel Forest
                    new EncounterSlot5 { Species = 434, LevelMin = 15, LevelMax = 60 }, // Stunky from Funfest Missions
                }).ToArray(),
            }
        };

        private static readonly EncounterArea5[] SlotsW2_HiddenGrotto =
        {
            new EncounterArea5
            {
                Location = 143, // Hidden Grotto
                Slots = SlotsB2W2_HiddenGrottoEncounterSlots.Concat(new[]{
                    new EncounterSlot5 { Species = 012, LevelMin = 55, LevelMax = 60 }, // Butterfree @ Pinwheel Forest
                    new EncounterSlot5 { Species = 431, LevelMin = 15, LevelMax = 60 }, // Glameow from Funfest Missions
                }).ToArray(),
            }
        };

        #endregion
    }
}
