using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper
{
    public static class Dumper4
    {
        public static void DumpGen4()
        {
            var d = Resources.encounter_d;
            var p = Resources.encounter_p;
            var pt = Resources.encounter_pt;

            var hg = Resources.encounter_hg;
            var ss = Resources.encounter_ss;

            var hb_hg = Resources.encounter_hb_hg;
            var hb_ss = Resources.encounter_hb_ss;

            var D_Slots = EncounterArea4DPPt.GetArray4DPPt(BinLinker.Unpack(d, "da"));
            var P_Slots = EncounterArea4DPPt.GetArray4DPPt(BinLinker.Unpack(p, "pe"));
            var Pt_Slots = EncounterArea4DPPt.GetArray4DPPt(BinLinker.Unpack(pt, "pt"), true);
            var HG_Slots = EncounterArea4HGSS.GetArray4HGSS(BinLinker.Unpack(hg, "hg"));
            var SS_Slots = EncounterArea4HGSS.GetArray4HGSS(BinLinker.Unpack(ss, "ss"));

            var DP_Feebas = GetFeebasArea(D_Slots[55], D_Slots[56], D_Slots[57]);
            var Pt_Feebas = GetFeebasArea(Pt_Slots[55], Pt_Slots[56], Pt_Slots[57]);
            var HG_Headbutt_Slots = EncounterArea4HGSS.GetArray4HGSS_Headbutt(BinLinker.Unpack(hb_hg, "hg"));
            var SS_Headbutt_Slots = EncounterArea4HGSS.GetArray4HGSS_Headbutt(BinLinker.Unpack(hb_ss, "ss"));

            var D_HoneyTrees_Slots = SlotsD_HoneyTree.Split(HoneyTreesLocation);
            var P_HoneyTrees_Slots = SlotsP_HoneyTree.Split(HoneyTreesLocation);
            var Pt_HoneyTrees_Slots = SlotsPt_HoneyTree.Split(HoneyTreesLocation);

            MarkG4SwarmSlots(HG_Slots, SlotsHG_Swarm);
            MarkG4SwarmSlots(SS_Slots, SlotsSS_Swarm);

            MarkEncounterTypeData(D_Slots, P_Slots, Pt_Slots, HG_Slots, SS_Slots);

            MarkG4SlotsGreatMarsh(D_Slots, 52);
            MarkG4SlotsGreatMarsh(P_Slots, 52);
            MarkG4SlotsGreatMarsh(Pt_Slots, 52);

            var SlotsD = ArrayUtil.ConcatAll(D_Slots, D_HoneyTrees_Slots, DP_GreatMarshAlt, DP_Feebas);
            var SlotsP = ArrayUtil.ConcatAll(P_Slots, P_HoneyTrees_Slots, DP_GreatMarshAlt, DP_Feebas);
            var SlotsPt = ArrayUtil.ConcatAll(Pt_Slots, Pt_HoneyTrees_Slots, Pt_GreatMarshAlt, Pt_Feebas);
            var SlotsHG = ArrayUtil.ConcatAll(HG_Slots, HG_Headbutt_Slots, Encounters4Extra.SlotsHGSSAlt);
            var SlotsSS = ArrayUtil.ConcatAll(SS_Slots, SS_Headbutt_Slots, Encounters4Extra.SlotsHGSSAlt);

            MarkDPPtEncounterTypeSlots(SlotsD);
            MarkDPPtEncounterTypeSlots(SlotsP);
            MarkDPPtEncounterTypeSlots(SlotsPt);
            MarkHGSSEncounterTypeSlots(SlotsHG);
            MarkHGSSEncounterTypeSlots(SlotsSS);

            // Remove inaccessible area slots
            // Johto Route 45 surfing encounter. Unreachable Water tiles.
            SlotsHG = SlotsHG.Where(z => z.Location != 193 || z.Type != SlotType.Surf).ToArray();
            SlotsSS = SlotsSS.Where(z => z.Location != 193 || z.Type != SlotType.Surf).ToArray();

            Write(SlotsD, "encounter_d.pkl", "da");
            Write(SlotsP, "encounter_p.pkl", "pe");
            Write(SlotsPt, "encounter_pt.pkl", "pt");
            Write(SlotsHG, "encounter_hg.pkl", "hg");
            Write(SlotsSS, "encounter_ss.pkl", "ss");
        }

        public static void Write(IEnumerable<EncounterArea4> area, string name, string ident = "g4")
        {
            area = area.OrderBy(z => z.Location).ThenBy(z => z.Type).ThenBy(z => z.TypeEncounter);
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

        public static byte[] Write(EncounterArea4 area)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write((ushort)area.Location);
            bw.Write((byte)area.Type);
            bw.Write((byte)area.Rate);
            bw.Write((ushort)area.TypeEncounter);

            foreach (var slot in area.Slots.Cast<EncounterSlot4>())
                WriteSlot(bw, slot);

            return ms.ToArray();
        }

        private static void WriteSlot(BinaryWriter bw, EncounterSlot4 slot)
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

        private static readonly int[] DP_GreatMarshAlt_Species =
        {
            // Daily changing Pokemon are not in the raw data http://bulbapedia.bulbagarden.net/wiki/Great_Marsh
            055, 315, 397, 451, 453, 455,
            183, 194, 195, 298, 399, 400,          // Pre-National Pokédex
            046, 102, 115, 193, 285, 316, 452, 454 // Post-National Pokédex
        };

        private static readonly EncounterArea4DPPt[] DP_GreatMarshAlt = EncounterArea.GetSimpleEncounterArea<EncounterArea4DPPt, EncounterSlot4>(DP_GreatMarshAlt_Species, new[] { 22, 22, 24, 24, 26, 26 }, 52, SlotType.Grass_Safari);

        private static readonly int[] Pt_GreatMarshAlt_Species =
        {
            114,193,195,357,451,453,455,
            194,                            // Pre-National Pokédex
            046,102,115,285,316,352,452,454 // Post-National Pokédex
        };

        private static readonly EncounterArea4DPPt[] Pt_GreatMarshAlt = EncounterArea.GetSimpleEncounterArea<EncounterArea4DPPt, EncounterSlot4>(Pt_GreatMarshAlt_Species, new[] { 27, 30 }, 52, SlotType.Grass_Safari);

        private static readonly EncounterArea4DPPt SlotsPt_HoneyTree = new EncounterArea4DPPt
        {
            Type = SlotType.HoneyTree,
            Slots = new[]
            {
                new EncounterSlot4 {Species = 190, LevelMin = 5, LevelMax = 15}, // Aipom
                new EncounterSlot4 {Species = 214, LevelMin = 5, LevelMax = 15}, // Heracross
                new EncounterSlot4 {Species = 265, LevelMin = 5, LevelMax = 15}, // Wurmple
                new EncounterSlot4 {Species = 412, LevelMin = 5, LevelMax = 15, Form = 0}, // Burmy Plant Cloak
                new EncounterSlot4 {Species = 415, LevelMin = 5, LevelMax = 15}, // Combee
                new EncounterSlot4 {Species = 420, LevelMin = 5, LevelMax = 15}, // Cheruby
                new EncounterSlot4 {Species = 446, LevelMin = 5, LevelMax = 15}, // Munchlax
            },
        };

        private static readonly EncounterArea4DPPt SlotsD_HoneyTree = new EncounterArea4DPPt
        {
            Type = SlotType.HoneyTree,
            Slots = SlotsPt_HoneyTree.Slots.Concat(new[]
            {
                new EncounterSlot4 {Species = 266, LevelMin = 5, LevelMax = 15}, // Silcoon
            }).ToArray()
        };

        private static readonly EncounterArea4DPPt SlotsP_HoneyTree = new EncounterArea4DPPt
        {
            Type = SlotType.HoneyTree,
            Slots = SlotsPt_HoneyTree.Slots.Concat(new[]
            {
                new EncounterSlot4 {Species = 268, LevelMin = 5, LevelMax = 15}, // Cascoon
            }).ToArray()
        };

        private static EncounterArea4DPPt[] GetFeebasArea(params EncounterArea4DPPt[] areas)
        {
#if DEBUG
            Debug.Assert(areas.Last().Location == 50); // Mt Coronet
            Debug.Assert(areas.Last().Slots.Last().Species == (int)Species.Whiscash);
            Debug.Assert(areas.Last().Slots[0].Species == (int)Species.Gyarados);

            Debug.Assert(areas.Length == 3);
            Debug.Assert(areas[0].Type == SlotType.Old_Rod);
            Debug.Assert(areas[1].Type == SlotType.Good_Rod);
            Debug.Assert(areas[2].Type == SlotType.Super_Rod);
#endif
            var result = new EncounterArea4DPPt[3];
            for (int i = 0; i < result.Length; i++)
            {
                var template = areas[i];
                var slots = template.Slots.Select(z => z.Clone()).ToArray();
                foreach (var s in slots)
                    s.Species = (int)Species.Feebas;

                var area = new EncounterArea4DPPt
                {
                    Location = template.Location,
                    Type = template.Type,
                    TypeEncounter = EncounterType.Surfing_Fishing,
                    Slots = slots,
                };
                result[i] = area;
            }

            return result;
        }

        private static readonly int[] Shellos_EastSeaLocation_DP =
        {
            28, // Route 213
            39, // Route 224
        };

        private static readonly int[] Shellos_EastSeaLocation_Pt =
        {
            11, // Pastoria City
            27, // Route 212
            28, // Route 213
        };

        private static readonly int[] Gastrodon_EastSeaLocation_DP =
        {
            37, // Route 222
            39, // Route 224
            45, // Route 230
        };

        private static readonly int[] Gastrodon_EastSeaLocation_Pt =
        {
            11, // Pastoria City
            27, // Route 212
            28, // Route 213
            39, // Route 224
            45, // Route 230
        };

        private static readonly int[] HoneyTreesLocation =
        {
            20, // Route 205
            21, // Route 206
            22, // Route 207
            23, // Route 208
            24, // Route 209
            25, // Route 210
            26, // Route 211
            27, // Route 212
            28, // Route 213
            29, // Route 214
            30, // Route 215
            33, // Route 218
            36, // Route 221
            37, // Route 222
            47, // Valley Windworks
            48, // Eterna Forest
            49, // Fuego Ironworks
            58, // Floaroma Meadow
        };

        private static readonly int[] MetLocationSolaceonRuins = { 53 };
        private static readonly int[] MetLocationRuinsOfAlph = { 209 };

        private static void MarkEncounterTypeData(EncounterArea4[] D_Slots, EncounterArea4[] P_Slots, EncounterArea4[] Pt_Slots, EncounterArea4[] HG_Slots, EncounterArea4[] SS_Slots)
        {
            // Shellos & Gastrodon
            MarkG4AltFormSlots(D_Slots, 422, 1, Shellos_EastSeaLocation_DP);
            MarkG4AltFormSlots(D_Slots, 423, 1, Gastrodon_EastSeaLocation_DP);
            MarkG4AltFormSlots(P_Slots, 422, 1, Shellos_EastSeaLocation_DP);
            MarkG4AltFormSlots(P_Slots, 423, 1, Gastrodon_EastSeaLocation_DP);
            MarkG4AltFormSlots(Pt_Slots, 422, 1, Shellos_EastSeaLocation_Pt);
            MarkG4AltFormSlots(Pt_Slots, 423, 1, Gastrodon_EastSeaLocation_Pt);

            MarkG4AltFormSlots( D_Slots, 201, 31, MetLocationSolaceonRuins);
            MarkG4AltFormSlots( P_Slots, 201, 31, MetLocationSolaceonRuins);
            MarkG4AltFormSlots(Pt_Slots, 201, 31, MetLocationSolaceonRuins);
            MarkG4AltFormSlots(HG_Slots, 201, 31, MetLocationRuinsOfAlph);
            MarkG4AltFormSlots(SS_Slots, 201, 31, MetLocationRuinsOfAlph);

            const int Route209 = 24;
            const int StarkMountain = 84;
            const int MtCoronet = 50;
            const int RuinsOfAlph = 209;
            const int MtSilver = 219;
            const int Cianwood = 130;
            MarkDPPtEncounterTypeSlots_MultipleTypes(D_Slots, Route209, EncounterType.Building_EnigmaStone, 0); // Exterior slots (Starly); not Lost Tower tables.
            MarkDPPtEncounterTypeSlots_MultipleTypes(P_Slots, Route209, EncounterType.Building_EnigmaStone, 0); // Exterior slots (Starly); not Lost Tower tables.
            MarkDPPtEncounterTypeSlots_MultipleTypes(Pt_Slots, Route209, EncounterType.Building_EnigmaStone, 0); // Exterior slots (Starly); not Lost Tower tables.
            MarkDPPtEncounterTypeSlots_MultipleTypes(D_Slots, StarkMountain, EncounterType.Cave_HallOfOrigin, 0); // Stark Mountain Camerupt
            MarkDPPtEncounterTypeSlots_MultipleTypes(P_Slots, StarkMountain, EncounterType.Cave_HallOfOrigin, 0); // Stark Mountain Camerupt
            MarkDPPtEncounterTypeSlots_MultipleTypes(Pt_Slots, StarkMountain, EncounterType.Cave_HallOfOrigin, 0); // Stark Mountain Camerupt
            MarkDPPtEncounterTypeSlots_MultipleTypes(D_Slots, MtCoronet, EncounterType.Cave_HallOfOrigin, DPPt_MtCoronetExteriorEncounters); // Snover land slots
            MarkDPPtEncounterTypeSlots_MultipleTypes(P_Slots, MtCoronet, EncounterType.Cave_HallOfOrigin, DPPt_MtCoronetExteriorEncounters); // Snover land slots
            MarkDPPtEncounterTypeSlots_MultipleTypes(Pt_Slots, MtCoronet, EncounterType.Cave_HallOfOrigin, DPPt_MtCoronetExteriorEncounters); // Snover land slots
            MarkHGSSEncounterTypeSlots_MultipleTypes(HG_Slots, RuinsOfAlph, EncounterType.Cave_HallOfOrigin, 0, 1, 2, 3, 4, 5); // Alph Exterior (not Unown)
            MarkHGSSEncounterTypeSlots_MultipleTypes(SS_Slots, RuinsOfAlph, EncounterType.Cave_HallOfOrigin, 0, 1, 2, 3, 4, 5); // Alph Exterior (not Unown)
            MarkHGSSEncounterTypeSlots_MultipleTypes(HG_Slots, MtSilver, EncounterType.Cave_HallOfOrigin, HGSS_MtSilverCaveExteriorEncounters); // Exterior
            MarkHGSSEncounterTypeSlots_MultipleTypes(SS_Slots, MtSilver, EncounterType.Cave_HallOfOrigin, HGSS_MtSilverCaveExteriorEncounters); // Exterior

            MarkHGSSEncounterTypeSlots_MultipleTypes(HG_Slots, Cianwood, EncounterType.RockSmash);
            MarkHGSSEncounterTypeSlots_MultipleTypes(SS_Slots, Cianwood, EncounterType.RockSmash);

            MarkSpecific(HG_Slots, RuinsOfAlph, SlotType.Rock_Smash, EncounterType.DialgaPalkia);
            MarkSpecific(SS_Slots, RuinsOfAlph, SlotType.Rock_Smash, EncounterType.DialgaPalkia);
        }

        private static void MarkG4SlotsGreatMarsh(IEnumerable<EncounterArea> Areas, int location)
        {
            foreach (EncounterArea Area in Areas.Where(a => a.Location == location))
                Area.Type |= SlotType.Safari;
        }

        private static void MarkG4SwarmSlots(IReadOnlyList<EncounterArea4HGSS> Areas, SwarmDef[] swarms)
        {
            // for fishing replace one or several random slots from encounters data, but all slots have the same level, it's ok to only replace the first
            // Species id are not included in encounter tables but levels can be copied from the encounter raw data
            foreach (var swarm in swarms)
            {
                var tables = Areas.Where(a => a.Location == swarm.Location && a.Type == swarm.Type).ToArray();
                var area = tables[swarm.TableIndex];
                var extra = new List<EncounterSlot4>();

                var indexes = GetSwarmSlotIndexes(swarm.Type);
                foreach (var index in indexes)
                {
                    var c0 = (EncounterSlot4)area.Slots[index].Clone();
                    if (index != c0.SlotNumber)
                        throw new Exception();

                    c0.Species = swarm.Species;
                    c0.Form = 0;
                    extra.Add(c0);

                    if (swarm.Species == (int)Species.Mawile
                    ) // edge case, Mawile is only swarm subject to magnet pull (no other steel types in area)
                    {
                        c0.MagnetPullIndex = c0.SlotNumber;
                        c0.MagnetPullCount = 2;
                    }
                }

                if (extra.Count == 0)
                    throw new Exception();

                area.Slots = area.Slots.Concat(extra).Cast<EncounterSlot4>().OrderBy(z => z.SlotNumber).ToArray();
            }
        }

        private static int[] GetSwarmSlotIndexes(SlotType type)
        {
            return type switch
            {
                // Grass Swarm slots replace slots 0 and 1 from encounters data
                SlotType.Grass => new[] {0, 1},
                // for surfing only replace slots 0 from encounters data
                SlotType.Surf => new[] {0},
                SlotType.Old_Rod => new[] {2},
                SlotType.Good_Rod => new[] {0, 2, 3},
                SlotType.Super_Rod => new[] {0, 1, 2, 3, 4}, // all
                _ => throw new Exception()
            };
        }

        // Gen 4 raw encounter data does not contains info for alt slots
        // Shellos and Gastrodon East Sea form should be modified
        private static void MarkG4AltFormSlots(IEnumerable<EncounterArea4> Areas, int Species, int form, int[] Locations)
        {
            foreach (var Area in Areas.Where(a => Locations.Contains(a.Location)))
            {
                foreach (EncounterSlot Slot in Area.Slots.Where(s => s.Species == Species))
                    Slot.Form = form;
            }
        }

        private static EncounterType GetEncounterTypeBySlotDPPt(SlotType Type, EncounterType GrassType)
        {
            return Type switch
            {
                SlotType.Swarm => GrassType,
                SlotType.Grass => GrassType,
                SlotType.Surf => EncounterType.Surfing_Fishing,
                SlotType.Old_Rod => EncounterType.Surfing_Fishing,
                SlotType.Good_Rod => EncounterType.Surfing_Fishing,
                SlotType.Super_Rod => EncounterType.Surfing_Fishing,
                SlotType.Surf_Safari => EncounterType.Surfing_Fishing,
                SlotType.Old_Rod_Safari => EncounterType.Surfing_Fishing,
                SlotType.Good_Rod_Safari => EncounterType.Surfing_Fishing,
                SlotType.Super_Rod_Safari => EncounterType.Surfing_Fishing,
                SlotType.Grass_Safari => EncounterType.MarshSafari,
                SlotType.HoneyTree => EncounterType.None,
                _ => EncounterType.None
            };
        }

        private static EncounterType GetEncounterTypeBySlotHGSS(SlotType Type, EncounterType GrassType, EncounterType HeadbuttType)
        {
            switch (Type)
            {
                // HGSS Safari encounters have normal water/grass encounter type, not safari encounter type
                case SlotType.Grass:
                case SlotType.Grass_Safari:
                case SlotType.BugContest: return GrassType;

                case SlotType.Surf:
                case SlotType.Old_Rod:
                case SlotType.Good_Rod:
                case SlotType.Super_Rod:
                case SlotType.Surf_Safari:
                case SlotType.Old_Rod_Safari:
                case SlotType.Good_Rod_Safari:
                case SlotType.Super_Rod_Safari: return EncounterType.Surfing_Fishing;

                case SlotType.Rock_Smash:
                    if (GrassType == EncounterType.RockSmash)
                        return EncounterType.RockSmash | EncounterType.Building_EnigmaStone;
                    if (HeadbuttType == EncounterType.Building_EnigmaStone)
                        return HeadbuttType;
                    if (GrassType == EncounterType.Cave_HallOfOrigin)
                        return GrassType;
                    return EncounterType.None;

                case SlotType.Headbutt | SlotType.Special:
                case SlotType.Headbutt: return HeadbuttType | EncounterType.None;
                    // not sure on if "None" should always be allowed, but this is so uncommon it shouldn't matter (gen7 doesn't keep this value anyway).
            }
            return EncounterType.None;
        }

        private static void MarkDPPtEncounterTypeSlots_MultipleTypes(EncounterArea4[] Areas, int Location, EncounterType NormalEncounterType, params int[] tallGrassAreaIndexes)
        {
            var numfile = 0;
            var areas = Areas.Where(x => x.Location == Location).ToArray();
            foreach (var area in areas)
            {
                var GrassType = tallGrassAreaIndexes.Contains(numfile) ? EncounterType.TallGrass : NormalEncounterType;
                area.TypeEncounter = GetEncounterTypeBySlotDPPt(area.Type, GrassType);
                numfile++;
            }
        }

        private static void MarkHGSSEncounterTypeSlots_MultipleTypes(EncounterArea4[] Areas, int Location, EncounterType NormalEncounterType, params int[] tallGrassAreaIndexes)
        {
            // Area with two different encounter type for grass encounters
            // SpecialEncounterFile is tall grass encounter type, the other files have the normal encounter type for this location
            var HeadbuttType = GetHeadbuttEncounterType(Location);
            var numfile = 0;
            var areas = Areas.Where(x => x.Location == Location).ToArray();
            foreach (var area in areas)
            {
                var GrassType = tallGrassAreaIndexes.Contains(numfile) ? EncounterType.TallGrass : NormalEncounterType;
                area.TypeEncounter = GetEncounterTypeBySlotHGSS(area.Type, GrassType, HeadbuttType);
                numfile++;
            }
        }

        private static void MarkSpecific(EncounterArea4[] Areas, int Location, SlotType t, EncounterType val)
        {
            var areas = Areas.Where(x => x.Location == Location && x.Type == t);
            foreach (var area in areas)
                area.TypeEncounter = val;
        }

        private static void MarkDPPtEncounterTypeSlots(EncounterArea4[] Areas)
        {
            foreach (var Area in Areas)
            {
                if (DPPt_MixInteriorExteriorLocations.Contains(Area.Location))
                    continue;

                var GrassType = GetGrassType(Area.Location);

                EncounterType GetGrassType(int location)
                {
                    if (location == 70) // Old Chateau
                        return EncounterType.Building_EnigmaStone;
                    if (DPPt_CaveLocations.Contains(Area.Location))
                        return EncounterType.Cave_HallOfOrigin;
                    return EncounterType.TallGrass;
                }

                if (Area.TypeEncounter == EncounterType.None) // not defined yet
                    Area.TypeEncounter = GetEncounterTypeBySlotDPPt(Area.Type, GrassType);
            }
        }

        private static EncounterType GetHeadbuttEncounterType(int Location)
        {
            if (Location == 195 || Location == 196) // Route 47/48
                return EncounterType.DialgaPalkia | EncounterType.TallGrass;

            // Routes with trees adjacent to water tiles
            var allowsurf = HGSS_SurfingHeadbutt_Locations.Contains(Location);

            // Cities
            if (HGSS_CityLocations.Contains(Location))
            {
                return allowsurf
                    ? EncounterType.Building_EnigmaStone | EncounterType.Surfing_Fishing
                    : EncounterType.Building_EnigmaStone;
            }

            // Caves with no exterior zones
            if (!HGSS_MixInteriorExteriorLocations.Contains(Location) && HGSS_CaveLocations.Contains(Location))
            {
                return allowsurf
                    ? EncounterType.Cave_HallOfOrigin | EncounterType.Surfing_Fishing
                    : EncounterType.Cave_HallOfOrigin;
            }

            // Routes and exterior areas
            // Routes with trees adjacent to grass tiles
            var allowgrass = HGSS_GrassHeadbutt_Locations.Contains(Location);
            if (allowgrass)
            {
                return allowsurf
                    ? EncounterType.TallGrass | EncounterType.Surfing_Fishing
                    : EncounterType.TallGrass;
            }

            return allowsurf
                ? EncounterType.Surfing_Fishing
                : EncounterType.None;
        }

        private static void MarkHGSSEncounterTypeSlots(EncounterArea4[] Areas)
        {
            foreach (var Area in Areas)
            {
                if (HGSS_MixInteriorExteriorLocations.Contains(Area.Location))
                    continue;
                var GrassType = HGSS_CaveLocations.Contains(Area.Location) ? EncounterType.Cave_HallOfOrigin : EncounterType.TallGrass;
                var HeadbuttType = GetHeadbuttEncounterType(Area.Location);

                if (Area.TypeEncounter == EncounterType.None) // not defined yet
                    Area.TypeEncounter = GetEncounterTypeBySlotHGSS(Area.Type, GrassType, HeadbuttType);
            }
        }

        #region Encounter Types
        private static readonly HashSet<int> DPPt_CaveLocations = new HashSet<int>
        {
            46, // Oreburgh Mine
            50, // Mt. Coronet
            53, // Solaceon Ruins
            54, // Sinnoh Victory Road
            57, // Ravaged Path
            59, // Oreburgh Gate
            62, // Turnback Cave
            64, // Snowpoint Temple
            65, // Wayward Cave
            66, // Ruin Maniac Cave
            67, // Maniac Tunnel
            66, // Ruin Maniac Cave
            69, // Iron Island
            84, // Stark Mountain
        };

        private static readonly HashSet<int> DPPt_MixInteriorExteriorLocations = new HashSet<int>
        {
            24, // Route 209 (Lost Tower)
            50, // Mt Coronet
            84, // Stark Mountain
        };

        private static readonly int[] DPPt_MtCoronetExteriorEncounters =
        {
            7, 8
        };

        /// <summary>
        /// Locations with headbutt trees accessible from Cave tiles
        /// </summary>
        private static readonly HashSet<int> HGSS_CaveLocations = new HashSet<int>
        {
            197, // DIGLETT's Cave
            198, // Mt. Moon
            199, // Cerulean Cave
            200, // Rock Tunnel
            201, // Power Plant
            203, // Seafoam Islands
            204, // Sprout Tower
            205, // Bell Tower
            206, // Burned Tower
            209, // Ruins of Alph
            210, // Union Cave
            211, // SLOWPOKE Well
            214, // Ilex Forest
            216, // Mt. Mortar
            217, // Ice Path
            218, // Whirl Islands
            219, // Mt. Silver Cave
            220, // Dark Cave
            221, // Kanto Victory Road
            223, // Tohjo Falls
            228, // Cliff Cave
            234, // Cliff Edge Gate
        };

        /// <summary>
        /// Locations with headbutt trees accessible from city tiles
        /// </summary>
        private static readonly HashSet<int> HGSS_CityLocations = new HashSet<int>
        {
            126, // New Bark Town
            127, // Cherrygrove City
            128, // Violet City
            129, // Azalea Town
            130, // Cianwood City
            131, // Goldenrod City
            132, // Olivine City
            133, // Ecruteak City
            134, // Mahogany Town
            136, // Blackthorn City
            138, // Pallet Town
            139, // Viridian City
            140, // Pewter City
            141, // Cerulean City
            142, // Lavender Town
            143, // Vermilion City
            144, // Celadon City
            145, // Fuchsia City
            146, // Cinnabar Island
            147, // Indigo Plateau
            148, // Saffron City
            227, // Safari Zone Gate
        };

        /// <summary>
        /// Locations with headbutt trees accessible from water tiles
        /// </summary>
        private static readonly HashSet<int> HGSS_SurfingHeadbutt_Locations = new HashSet<int>
        {
            126, // New Bark Town
            127, // Cherrygrove City
            128, // Violet City
            133, // Ecruteak City
            135, // Lake of Rage
            138, // Pallet Town
            139, // Viridian City
            160, // Route 12
            169, // Route 21
            170, // Route 22
            174, // Route 26
            175, // Route 27
            176, // Route 28
            178, // Route 30
            179, // Route 31
            180, // Route 32
            182, // Route 34
            183, // Route 35
            190, // Route 42
            191, // Route 43
            192, // Route 44
            214, // Ilex Forest
        };

        /// <summary>
        /// Locations with headbutt trees accessible from tall grass tiles
        /// </summary>
        private static readonly HashSet<int> HGSS_GrassHeadbutt_Locations = new HashSet<int>
        {
            137, // Mt. Silver
            149, // Route 1
            150, // Route 2
            151, // Route 3
            152, // Route 4
            153, // Route 5
            154, // Route 6
            155, // Route 7
            159, // Route 11
            161, // Route 13
            163, // Route 15
            164, // Route 16
            169, // Route 21
            170, // Route 22
            174, // Route 26
            175, // Route 27
            176, // Route 28
            177, // Route 29
            178, // Route 30
            179, // Route 31
            180, // Route 32
            181, // Route 33
            182, // Route 34
            183, // Route 35
            184, // Route 36
            185, // Route 37
            186, // Route 38
            187, // Route 39
            191, // Route 43
            192, // Route 44
            194, // Route 46
            195, // Route 47
            196, // Route 48
            219, // Mt. Silver Cave
            224, // Viridian Forest
        };

        private static readonly int[] HGSS_MtSilverCaveExteriorEncounters =
        {
            5, 10
        };

        private static readonly int[] HGSS_MixInteriorExteriorLocations =
        {
            209, // Ruins of Alph
            219, // Mt. Silver Cave
        };

        private class SwarmDef
        {
            public readonly short Location;
            public readonly int Species;
            public readonly SlotType Type;
            public readonly int TableIndex;

            public SwarmDef(short location, int species, SlotType type, int index = 0)
            {
                Location = location;
                Species = species;
                Type = type;
                TableIndex = index;
            }

            public override string ToString() => $"{Location}{(TableIndex == 0 ? "" : $"({TableIndex})")} - {(Species) Species}";
        }

        private static readonly SwarmDef[] SlotsHGSS_Swarm =
        {
            new SwarmDef(143, 278, SlotType.Surf ), // Wingull @ Vermillion City
            new SwarmDef(149, 261, SlotType.Grass), // Poochyena @ Route 1
            new SwarmDef(161, 113, SlotType.Grass), // Chansey @ Route 13
            new SwarmDef(167, 366, SlotType.Surf ), // Clamperl @ Route 19
            new SwarmDef(173, 427, SlotType.Grass), // Buneary @ Route 25
            new SwarmDef(175, 370, SlotType.Surf ), // Luvdisc @ Route 27
            new SwarmDef(182, 280, SlotType.Grass), // Ralts @ Route 34
            new SwarmDef(183, 193, SlotType.Grass), // Yanma @ Route 35
            new SwarmDef(186, 209, SlotType.Grass), // Snubbull @ Route 38
            new SwarmDef(193, 333, SlotType.Grass), // Swablu @ Route 45
            new SwarmDef(195, 132, SlotType.Grass), // Ditto @ Route 47
            new SwarmDef(216, 183, SlotType.Grass), // Marill @ Mt. Mortar
            new SwarmDef(220, 206, SlotType.Grass, 1), // Dunsparce @ Dark Cave (Route 31 side; the r45 does not have Dunsparce swarm)
            new SwarmDef(224, 401, SlotType.Grass), // Kricketot @ Viridian Forest

            new SwarmDef(128, 340, SlotType.Old_Rod),   // Whiscash @ Violet City
            new SwarmDef(128, 340, SlotType.Good_Rod),  // Whiscash @ Violet City
            new SwarmDef(128, 340, SlotType.Super_Rod), // Whiscash @ Violet City

            new SwarmDef(160, 369, SlotType.Old_Rod),   // Relicanth @ Route 12
            new SwarmDef(160, 369, SlotType.Good_Rod),  // Relicanth @ Route 12
            new SwarmDef(160, 369, SlotType.Super_Rod), // Relicanth @ Route 12

            new SwarmDef(180, 211, SlotType.Old_Rod),   // Qwilfish @ Route 32
            new SwarmDef(180, 211, SlotType.Good_Rod),  // Qwilfish @ Route 32
            new SwarmDef(180, 211, SlotType.Super_Rod), // Qwilfish @ Route 32

            new SwarmDef(192, 223, SlotType.Old_Rod),   // Remoraid @ Route 44
            new SwarmDef(192, 223, SlotType.Good_Rod),  // Remoraid @ Route 44
            new SwarmDef(192, 223, SlotType.Super_Rod), // Remoraid @ Route 44
        };

        private static readonly SwarmDef[] SlotsHG_Swarm = SlotsHGSS_Swarm.Concat(new[] {
            new SwarmDef(151, 343, SlotType.Grass), // Baltoy @ Route 3
            new SwarmDef(157, 302, SlotType.Grass), // Sableye @ Route 9
        }).ToArray();

        private static readonly SwarmDef[] SlotsSS_Swarm = SlotsHGSS_Swarm.Concat(new[] {
            new SwarmDef(151, 316, SlotType.Grass), // Gulpin @ Route 3
            new SwarmDef(157, 303, SlotType.Grass), // Mawile @ Route 9
        }).ToArray();
        #endregion
    }

    public static class Encounters4Extra
    {
        private static readonly EncounterArea4HGSS BCC_PreNational = new EncounterArea4HGSS
        {
            Location = 207, // National Park Catching Contest
            Type = SlotType.BugContest,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 010, LevelMin = 07, LevelMax = 18, SlotNumber = 0 }, // Caterpie
                new EncounterSlot4 { Species = 013, LevelMin = 07, LevelMax = 18, SlotNumber = 1 }, // Weedle
                new EncounterSlot4 { Species = 011, LevelMin = 09, LevelMax = 18, SlotNumber = 2 }, // Metapod
                new EncounterSlot4 { Species = 014, LevelMin = 09, LevelMax = 18, SlotNumber = 3 }, // Kakuna
                new EncounterSlot4 { Species = 012, LevelMin = 12, LevelMax = 15, SlotNumber = 4 }, // Butterfree
                new EncounterSlot4 { Species = 015, LevelMin = 12, LevelMax = 15, SlotNumber = 5 }, // Beedrill
                new EncounterSlot4 { Species = 048, LevelMin = 10, LevelMax = 16, SlotNumber = 6 }, // Venonat
                new EncounterSlot4 { Species = 046, LevelMin = 10, LevelMax = 17, SlotNumber = 7 }, // Paras
                new EncounterSlot4 { Species = 123, LevelMin = 13, LevelMax = 14, SlotNumber = 8 }, // Scyther
                new EncounterSlot4 { Species = 127, LevelMin = 13, LevelMax = 14, SlotNumber = 9 }, // Pinsir
            }
        };

        private static readonly EncounterArea4HGSS BCC_PostTuesday = new EncounterArea4HGSS
        {
            Location = 207, // National Park Catching Contest
            Type = SlotType.BugContest,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 010, LevelMin = 24, LevelMax = 36, SlotNumber = 0 }, // Caterpie
                new EncounterSlot4 { Species = 013, LevelMin = 24, LevelMax = 36, SlotNumber = 1 }, // Weedle
                new EncounterSlot4 { Species = 011, LevelMin = 26, LevelMax = 36, SlotNumber = 2 }, // Metapod
                new EncounterSlot4 { Species = 014, LevelMin = 26, LevelMax = 36, SlotNumber = 3 }, // Kakuna
                new EncounterSlot4 { Species = 012, LevelMin = 27, LevelMax = 30, SlotNumber = 4 }, // Butterfree
                new EncounterSlot4 { Species = 015, LevelMin = 27, LevelMax = 30, SlotNumber = 5 }, // Beedrill
                new EncounterSlot4 { Species = 048, LevelMin = 25, LevelMax = 32, SlotNumber = 6 }, // Venonat
                new EncounterSlot4 { Species = 046, LevelMin = 27, LevelMax = 34, SlotNumber = 7 }, // Paras
                new EncounterSlot4 { Species = 123, LevelMin = 27, LevelMax = 28, SlotNumber = 8 }, // Scyther
                new EncounterSlot4 { Species = 127, LevelMin = 27, LevelMax = 28, SlotNumber = 9 }, // Pinsir
            }
        };

        private static readonly EncounterArea4HGSS BCC_PostThursday = new EncounterArea4HGSS
        {
            Location = 207, // National Park Catching Contest
            Type = SlotType.BugContest,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 265, LevelMin = 24, LevelMax = 36, SlotNumber = 0 }, // Wurmple
                new EncounterSlot4 { Species = 266, LevelMin = 24, LevelMax = 36, SlotNumber = 1 }, // Silcoon (Thursday)
                new EncounterSlot4 { Species = 290, LevelMin = 26, LevelMax = 36, SlotNumber = 2 }, // Nincada
                new EncounterSlot4 { Species = 313, LevelMin = 26, LevelMax = 36, SlotNumber = 3 }, // Volbeat (Thursday)
                new EncounterSlot4 { Species = 401, LevelMin = 27, LevelMax = 30, SlotNumber = 4 }, // Kricketot
                new EncounterSlot4 { Species = 402, LevelMin = 27, LevelMax = 30, SlotNumber = 5 }, // Kricketune
                new EncounterSlot4 { Species = 269, LevelMin = 25, LevelMax = 32, SlotNumber = 6 }, // Dustox (Thursday)
                new EncounterSlot4 { Species = 415, LevelMin = 27, LevelMax = 34, SlotNumber = 7 }, // Combee
                new EncounterSlot4 { Species = 123, LevelMin = 27, LevelMax = 28, SlotNumber = 8 }, // Scyther
                new EncounterSlot4 { Species = 127, LevelMin = 27, LevelMax = 28, SlotNumber = 9 }, // Pinsir
            }
        };

        private static readonly EncounterArea4HGSS BCC_PostSaturday = new EncounterArea4HGSS
        {
            Location = 207, // National Park Catching Contest
            Type = SlotType.BugContest,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 265, LevelMin = 24, LevelMax = 36, SlotNumber = 0 }, // Wurmple
                new EncounterSlot4 { Species = 268, LevelMin = 24, LevelMax = 36, SlotNumber = 1 }, // Cascoon (Saturday)
                new EncounterSlot4 { Species = 290, LevelMin = 26, LevelMax = 36, SlotNumber = 2 }, // Nincada
                new EncounterSlot4 { Species = 314, LevelMin = 26, LevelMax = 36, SlotNumber = 3 }, // Illumise (Saturday)
                new EncounterSlot4 { Species = 401, LevelMin = 27, LevelMax = 30, SlotNumber = 4 }, // Kricketot
                new EncounterSlot4 { Species = 402, LevelMin = 27, LevelMax = 30, SlotNumber = 5 }, // Kricketune
                new EncounterSlot4 { Species = 267, LevelMin = 25, LevelMax = 32, SlotNumber = 6 }, // Beautifly (Saturday)
                new EncounterSlot4 { Species = 415, LevelMin = 27, LevelMax = 34, SlotNumber = 7 }, // Combee
                new EncounterSlot4 { Species = 123, LevelMin = 27, LevelMax = 28, SlotNumber = 8 }, // Scyther
                new EncounterSlot4 { Species = 127, LevelMin = 27, LevelMax = 28, SlotNumber = 9 }, // Pinsir
            }
        };

        // Source http://bulbapedia.bulbagarden.net/wiki/Johto_Safari_Zone#Pok.C3.A9mon
        // Supplement http://www.psypokes.com/hgss/safari_areas.php
        private static readonly EncounterArea4HGSS SAFARIZONE_PEAK = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 022, LevelMin = 44, LevelMax = 44 }, // Fearow
                new EncounterSlot4 { Species = 046, LevelMin = 42, LevelMax = 42 }, // Paras
                new EncounterSlot4 { Species = 074, LevelMin = 15, LevelMax = 17 }, // Geodude
                new EncounterSlot4 { Species = 075, LevelMin = 16, LevelMax = 17 }, // Graveler
                new EncounterSlot4 { Species = 080, LevelMin = 45, LevelMax = 45 }, // Slowbro
                new EncounterSlot4 { Species = 081, LevelMin = 15, LevelMax = 16 }, // Magnemite
                new EncounterSlot4 { Species = 082, LevelMin = 17, LevelMax = 17 }, // Magneton
                new EncounterSlot4 { Species = 126, LevelMin = 17, LevelMax = 17 }, // Magmar
                new EncounterSlot4 { Species = 126, LevelMin = 41, LevelMax = 41 }, // Magmar
                new EncounterSlot4 { Species = 202, LevelMin = 16, LevelMax = 17 }, // Wobbuffet
                new EncounterSlot4 { Species = 202, LevelMin = 41, LevelMax = 41 }, // Wobbuffet
                new EncounterSlot4 { Species = 264, LevelMin = 46, LevelMax = 46 }, // Linoone
                new EncounterSlot4 { Species = 288, LevelMin = 47, LevelMax = 47 }, // Vigoroth
                new EncounterSlot4 { Species = 305, LevelMin = 45, LevelMax = 45 }, // Lairon
                new EncounterSlot4 { Species = 335, LevelMin = 43, LevelMax = 45 }, // Zangoose
                new EncounterSlot4 { Species = 363, LevelMin = 44, LevelMax = 45 }, // Spheal
                new EncounterSlot4 { Species = 436, LevelMin = 45, LevelMax = 46 }, // Bronzor
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_DESERT = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 022, LevelMin = 15, LevelMax = 17 }, // Fearow
                new EncounterSlot4 { Species = 022, LevelMin = 38, LevelMax = 38 }, // Fearow
                new EncounterSlot4 { Species = 022, LevelMin = 41, LevelMax = 41 }, // Fearow
                new EncounterSlot4 { Species = 027, LevelMin = 15, LevelMax = 17 }, // Sandshrew
                new EncounterSlot4 { Species = 028, LevelMin = 15, LevelMax = 17 }, // Sandslash
                new EncounterSlot4 { Species = 104, LevelMin = 16, LevelMax = 17 }, // Cubone
                new EncounterSlot4 { Species = 105, LevelMin = 17, LevelMax = 17 }, // Marowak
                new EncounterSlot4 { Species = 105, LevelMin = 41, LevelMax = 41 }, // Marowak
                new EncounterSlot4 { Species = 270, LevelMin = 38, LevelMax = 38 }, // Lotad
                new EncounterSlot4 { Species = 327, LevelMin = 45, LevelMax = 45 }, // Spinda
                new EncounterSlot4 { Species = 328, LevelMin = 46, LevelMax = 47 }, // Trapinch
                new EncounterSlot4 { Species = 329, LevelMin = 44, LevelMax = 45 }, // Vibrava
                new EncounterSlot4 { Species = 331, LevelMin = 35, LevelMax = 35 }, // Cacnea
                new EncounterSlot4 { Species = 332, LevelMin = 48, LevelMax = 48 }, // Cacturne
                new EncounterSlot4 { Species = 449, LevelMin = 43, LevelMax = 43 }, // Hippopotas
                new EncounterSlot4 { Species = 455, LevelMin = 48, LevelMax = 48 }, // Carnivine
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_PLAINS = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 019, LevelMin = 15, LevelMax = 17 }, // Rattata
                new EncounterSlot4 { Species = 020, LevelMin = 15, LevelMax = 17 }, // Raticate
                new EncounterSlot4 { Species = 063, LevelMin = 15, LevelMax = 17 }, // Abra
                new EncounterSlot4 { Species = 077, LevelMin = 42, LevelMax = 42 }, // Ponyta
                new EncounterSlot4 { Species = 203, LevelMin = 15, LevelMax = 17 }, // Girafarig
                new EncounterSlot4 { Species = 203, LevelMin = 40, LevelMax = 40 }, // Girafarig
                new EncounterSlot4 { Species = 229, LevelMin = 43, LevelMax = 44 }, // Houndoom
                new EncounterSlot4 { Species = 234, LevelMin = 17, LevelMax = 17 }, // Stantler
                new EncounterSlot4 { Species = 234, LevelMin = 40, LevelMax = 41 }, // Stantler
                new EncounterSlot4 { Species = 235, LevelMin = 17, LevelMax = 17 }, // Smeargle
                new EncounterSlot4 { Species = 235, LevelMin = 41, LevelMax = 41 }, // Smeargle
                new EncounterSlot4 { Species = 263, LevelMin = 44, LevelMax = 44 }, // Zigzagoon
                new EncounterSlot4 { Species = 270, LevelMin = 42, LevelMax = 42 }, // Lotad
                new EncounterSlot4 { Species = 283, LevelMin = 46, LevelMax = 46 }, // Surskit
                new EncounterSlot4 { Species = 310, LevelMin = 45, LevelMax = 45 }, // Manectric
                new EncounterSlot4 { Species = 335, LevelMin = 43, LevelMax = 45 }, // Zangoose
                new EncounterSlot4 { Species = 403, LevelMin = 43, LevelMax = 44 }, // Shinx
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_MEADOW_Grass = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 020, LevelMin = 40, LevelMax = 40 }, // Raticate
                new EncounterSlot4 { Species = 035, LevelMin = 17, LevelMax = 17 }, // Clefairy
                new EncounterSlot4 { Species = 035, LevelMin = 42, LevelMax = 42 }, // Clefairy
                new EncounterSlot4 { Species = 039, LevelMin = 15, LevelMax = 17 }, // Jigglypuff
                new EncounterSlot4 { Species = 074, LevelMin = 45, LevelMax = 45 }, // Geodude
                new EncounterSlot4 { Species = 113, LevelMin = 42, LevelMax = 42 }, // Chansey
                new EncounterSlot4 { Species = 187, LevelMin = 15, LevelMax = 17 }, // Hoppip
                new EncounterSlot4 { Species = 188, LevelMin = 17, LevelMax = 17 }, // Skiploom
                new EncounterSlot4 { Species = 188, LevelMin = 40, LevelMax = 40 }, // Skiploom
                new EncounterSlot4 { Species = 183, LevelMin = 15, LevelMax = 17 }, // Marill
                new EncounterSlot4 { Species = 191, LevelMin = 15, LevelMax = 17 }, // Sunkern
                new EncounterSlot4 { Species = 194, LevelMin = 15, LevelMax = 17 }, // Wooper
                new EncounterSlot4 { Species = 194, LevelMin = 40, LevelMax = 40 }, // Wooper
                new EncounterSlot4 { Species = 273, LevelMin = 45, LevelMax = 45 }, // Seedot
                new EncounterSlot4 { Species = 274, LevelMin = 38, LevelMax = 38 }, // Nuzleaf
                new EncounterSlot4 { Species = 274, LevelMin = 47, LevelMax = 48 }, // Nuzleaf
                new EncounterSlot4 { Species = 299, LevelMin = 45, LevelMax = 45 }, // Nosepass
                new EncounterSlot4 { Species = 447, LevelMin = 45, LevelMax = 46 }, // Riolu
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_MEADOW_Surf = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Surf_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 129, LevelMin = 15, LevelMax = 17 }, // Magikarp
                new EncounterSlot4 { Species = 183, LevelMin = 16, LevelMax = 17 }, // Marill
                new EncounterSlot4 { Species = 188, LevelMin = 47, LevelMax = 47 }, // Skiploom
                new EncounterSlot4 { Species = 194, LevelMin = 15, LevelMax = 17 }, // Wooper
                new EncounterSlot4 { Species = 284, LevelMin = 42, LevelMax = 42 }, // Masquerain
                new EncounterSlot4 { Species = 284, LevelMin = 46, LevelMax = 46 }, // Masquerain
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_MEADOW_Old = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Old_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 060, LevelMin = 12, LevelMax = 15 }, // Poliwag
                new EncounterSlot4 { Species = 061, LevelMin = 15, LevelMax = 16 }, // Poliwhirl
                new EncounterSlot4 { Species = 129, LevelMin = 12, LevelMax = 15 }, // Magikarp
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_MEADOW_Good = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Good_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 060, LevelMin = 22, LevelMax = 24 }, // Poliwag
                new EncounterSlot4 { Species = 061, LevelMin = 24, LevelMax = 25 }, // Poliwhirl
                new EncounterSlot4 { Species = 061, LevelMin = 27, LevelMax = 27 }, // Poliwhirl
                new EncounterSlot4 { Species = 129, LevelMin = 22, LevelMax = 24 }, // Magikarp
                new EncounterSlot4 { Species = 130, LevelMin = 28, LevelMax = 28 }, // Gyarados
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_MEADOW_Super = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Super_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 060, LevelMin = 35, LevelMax = 36 }, // Poliwag
                new EncounterSlot4 { Species = 061, LevelMin = 35, LevelMax = 38 }, // Poliwhirl
                new EncounterSlot4 { Species = 130, LevelMin = 42, LevelMax = 42 }, // Gyarados
                new EncounterSlot4 { Species = 130, LevelMin = 45, LevelMax = 45 }, // Gyarados
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_FOREST = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 016, LevelMin = 15, LevelMax = 17 }, // Pidgey
                new EncounterSlot4 { Species = 069, LevelMin = 15, LevelMax = 17 }, // Bellsprout
                new EncounterSlot4 { Species = 092, LevelMin = 15, LevelMax = 17 }, // Gastly
                new EncounterSlot4 { Species = 093, LevelMin = 16, LevelMax = 17 }, // Haunter
                new EncounterSlot4 { Species = 108, LevelMin = 40, LevelMax = 40 }, // Lickitung
                new EncounterSlot4 { Species = 122, LevelMin = 16, LevelMax = 17 }, // Mr. Mime
                new EncounterSlot4 { Species = 122, LevelMin = 45, LevelMax = 45 }, // Mr. Mime
                new EncounterSlot4 { Species = 125, LevelMin = 41, LevelMax = 41 }, // Electabuzz
                new EncounterSlot4 { Species = 200, LevelMin = 15, LevelMax = 17 }, // Misdreavus
                new EncounterSlot4 { Species = 200, LevelMin = 42, LevelMax = 42 }, // Misdreavus
                new EncounterSlot4 { Species = 283, LevelMin = 42, LevelMax = 42 }, // Surskit
                new EncounterSlot4 { Species = 353, LevelMin = 46, LevelMax = 47 }, // Shuppet
                new EncounterSlot4 { Species = 374, LevelMin = 44, LevelMax = 44 }, // Beldum
                new EncounterSlot4 { Species = 399, LevelMin = 40, LevelMax = 40 }, // Bidoof
                new EncounterSlot4 { Species = 406, LevelMin = 47, LevelMax = 47 }, // Budew
                new EncounterSlot4 { Species = 437, LevelMin = 44, LevelMax = 45 }, // Bronzong
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_SWAMP_Grass = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 039, LevelMin = 15, LevelMax = 17 }, // Jigglypuff
                new EncounterSlot4 { Species = 046, LevelMin = 15, LevelMax = 17 }, // Paras
                new EncounterSlot4 { Species = 047, LevelMin = 41, LevelMax = 41 }, // Parasect
                new EncounterSlot4 { Species = 070, LevelMin = 46, LevelMax = 46 }, // Weepinbell
                new EncounterSlot4 { Species = 096, LevelMin = 15, LevelMax = 17 }, // Drowzee
                new EncounterSlot4 { Species = 097, LevelMin = 16, LevelMax = 17 }, // Hypno
                new EncounterSlot4 { Species = 097, LevelMin = 37, LevelMax = 37 }, // Hypno
                new EncounterSlot4 { Species = 100, LevelMin = 42, LevelMax = 42 }, // Voltorb
                new EncounterSlot4 { Species = 161, LevelMin = 15, LevelMax = 17 }, // Sentret
                new EncounterSlot4 { Species = 162, LevelMin = 42, LevelMax = 42 }, // Furret
                new EncounterSlot4 { Species = 198, LevelMin = 15, LevelMax = 17 }, // Murkrow
                new EncounterSlot4 { Species = 198, LevelMin = 37, LevelMax = 37 }, // Murkrow
                new EncounterSlot4 { Species = 355, LevelMin = 38, LevelMax = 38 }, // Duskull
                new EncounterSlot4 { Species = 358, LevelMin = 46, LevelMax = 47 }, // Chimecho
                new EncounterSlot4 { Species = 371, LevelMin = 44, LevelMax = 45 }, // Bagon
                new EncounterSlot4 { Species = 417, LevelMin = 47, LevelMax = 47 }, // Pachirisu
                new EncounterSlot4 { Species = 419, LevelMin = 44, LevelMax = 44 }, // Floatzel
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_SWAMP_Surf = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Surf_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 118, LevelMin = 15, LevelMax = 17 }, // Goldeen
                new EncounterSlot4 { Species = 119, LevelMin = 42, LevelMax = 42 }, // Seaking
                new EncounterSlot4 { Species = 129, LevelMin = 15, LevelMax = 17 }, // Magikarp
                new EncounterSlot4 { Species = 198, LevelMin = 47, LevelMax = 47 }, // Murkrow
                new EncounterSlot4 { Species = 355, LevelMin = 48, LevelMax = 48 }, // Duskull
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_SWAMP_Old = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Old_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 118, LevelMin = 17, LevelMax = 17 }, // Goldeen
                new EncounterSlot4 { Species = 119, LevelMin = 17, LevelMax = 17 }, // Seaking
                new EncounterSlot4 { Species = 129, LevelMin = 12, LevelMax = 15 }, // Magikarp
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_SWAMP_Good = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Good_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 118, LevelMin = 22, LevelMax = 24 }, // Goldeen
                new EncounterSlot4 { Species = 119, LevelMin = 24, LevelMax = 25 }, // Seaking
                new EncounterSlot4 { Species = 119, LevelMin = 27, LevelMax = 27 }, // Seaking
                new EncounterSlot4 { Species = 129, LevelMin = 22, LevelMax = 24 }, // Magikarp
                new EncounterSlot4 { Species = 147, LevelMin = 29, LevelMax = 29 }, // Dratini
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_SWAMP_Super = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Super_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 118, LevelMin = 35, LevelMax = 37 }, // Goldeen
                new EncounterSlot4 { Species = 119, LevelMin = 35, LevelMax = 37 }, // Seaking
                new EncounterSlot4 { Species = 147, LevelMin = 36, LevelMax = 37 }, // Dratini
                new EncounterSlot4 { Species = 148, LevelMin = 42, LevelMax = 42 }, // Dragonair
                new EncounterSlot4 { Species = 148, LevelMin = 45, LevelMax = 45 }, // Dragonair
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_MARSHLAND_Grass = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 023, LevelMin = 15, LevelMax = 16 }, // Ekans
                new EncounterSlot4 { Species = 024, LevelMin = 17, LevelMax = 17 }, // Arbok
                new EncounterSlot4 { Species = 043, LevelMin = 15, LevelMax = 17 }, // Oddish
                new EncounterSlot4 { Species = 044, LevelMin = 16, LevelMax = 17 }, // Gloom
                new EncounterSlot4 { Species = 044, LevelMin = 42, LevelMax = 42 }, // Gloom
                new EncounterSlot4 { Species = 050, LevelMin = 43, LevelMax = 43 }, // Diglett
                new EncounterSlot4 { Species = 088, LevelMin = 17, LevelMax = 17 }, // Grimer
                new EncounterSlot4 { Species = 089, LevelMin = 38, LevelMax = 38 }, // Muk
                new EncounterSlot4 { Species = 109, LevelMin = 15, LevelMax = 17 }, // Koffing
                new EncounterSlot4 { Species = 110, LevelMin = 15, LevelMax = 17 }, // Weezing
                new EncounterSlot4 { Species = 189, LevelMin = 38, LevelMax = 38 }, // Jumpluff
                new EncounterSlot4 { Species = 194, LevelMin = 15, LevelMax = 17 }, // Wooper
                new EncounterSlot4 { Species = 213, LevelMin = 44, LevelMax = 44 }, // Shuckle
                new EncounterSlot4 { Species = 315, LevelMin = 46, LevelMax = 46 }, // Roselia
                new EncounterSlot4 { Species = 336, LevelMin = 47, LevelMax = 48 }, // Seviper
                new EncounterSlot4 { Species = 354, LevelMin = 44, LevelMax = 45 }, // Banette
                new EncounterSlot4 { Species = 453, LevelMin = 44, LevelMax = 44 }, // Croagunk
                new EncounterSlot4 { Species = 455, LevelMin = 41, LevelMax = 41 }, // Carnivine
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_MARSHLAND_Surf = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Surf_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 060, LevelMin = 15, LevelMax = 17 }, // Poliwag
                new EncounterSlot4 { Species = 088, LevelMin = 15, LevelMax = 17 }, // Grimer
                new EncounterSlot4 { Species = 089, LevelMin = 48, LevelMax = 48 }, // Muk
                new EncounterSlot4 { Species = 189, LevelMin = 47, LevelMax = 47 }, // Jumpluff
                new EncounterSlot4 { Species = 194, LevelMin = 15, LevelMax = 17 }, // Wooper
                new EncounterSlot4 { Species = 195, LevelMin = 43, LevelMax = 43 }, // Quagsire
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_MARSHLAND_Old = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Old_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 060, LevelMin = 12, LevelMax = 15 }, // Poliwag
                new EncounterSlot4 { Species = 060, LevelMin = 16, LevelMax = 16 }, // Poliwag
                new EncounterSlot4 { Species = 060, LevelMin = 18, LevelMax = 18 }, // Poliwag
                new EncounterSlot4 { Species = 129, LevelMin = 12, LevelMax = 15 }, // Magikarp
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_MARSHLAND_Good = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Good_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 061, LevelMin = 22, LevelMax = 25 }, // Poliwhirl
                new EncounterSlot4 { Species = 129, LevelMin = 22, LevelMax = 24 }, // Magikarp
                new EncounterSlot4 { Species = 130, LevelMin = 26, LevelMax = 26 }, // Gyarados
                new EncounterSlot4 { Species = 130, LevelMin = 29, LevelMax = 29 }, // Gyarados
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_MARSHLAND_Super = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Super_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 061, LevelMin = 35, LevelMax = 38 }, // Poliwhirl
                new EncounterSlot4 { Species = 130, LevelMin = 36, LevelMax = 37 }, // Gyarados
                new EncounterSlot4 { Species = 339, LevelMin = 42, LevelMax = 42 }, // Barboach
                new EncounterSlot4 { Species = 339, LevelMin = 45, LevelMax = 45 }, // Barboach
            },
        };
        private static readonly EncounterArea4HGSS SAFARIZONE_MOUNTAIN = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 019, LevelMin = 15, LevelMax = 16 }, // Rattata
                new EncounterSlot4 { Species = 020, LevelMin = 15, LevelMax = 17 }, // Raticate
                new EncounterSlot4 { Species = 041, LevelMin = 15, LevelMax = 17 }, // Zubat
                new EncounterSlot4 { Species = 042, LevelMin = 15, LevelMax = 17 }, // Golbat
                new EncounterSlot4 { Species = 082, LevelMin = 17, LevelMax = 17 }, // Magneton
                new EncounterSlot4 { Species = 082, LevelMin = 42, LevelMax = 42 }, // Magneton
                new EncounterSlot4 { Species = 098, LevelMin = 43, LevelMax = 43 }, // Krabby
                new EncounterSlot4 { Species = 108, LevelMin = 15, LevelMax = 17 }, // Lickitung
                new EncounterSlot4 { Species = 246, LevelMin = 17, LevelMax = 17 }, // Larvitar
                new EncounterSlot4 { Species = 246, LevelMin = 42, LevelMax = 42 }, // Larvitar
                new EncounterSlot4 { Species = 307, LevelMin = 43, LevelMax = 44 }, // Meditite
                new EncounterSlot4 { Species = 313, LevelMin = 46, LevelMax = 46 }, // Volbeat
                new EncounterSlot4 { Species = 337, LevelMin = 46, LevelMax = 46 }, // Lunatone
                new EncounterSlot4 { Species = 356, LevelMin = 45, LevelMax = 46 }, // Dusclops
                new EncounterSlot4 { Species = 364, LevelMin = 45, LevelMax = 45 }, // Sealeo
                new EncounterSlot4 { Species = 375, LevelMin = 44, LevelMax = 44 }, // Metang
                new EncounterSlot4 { Species = 433, LevelMin = 38, LevelMax = 38 }, // Chingling
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_ROCKYBEACH_Grass = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 041, LevelMin = 15, LevelMax = 17 }, // Zubat
                new EncounterSlot4 { Species = 079, LevelMin = 15, LevelMax = 17 }, // Slowpoke
                new EncounterSlot4 { Species = 080, LevelMin = 17, LevelMax = 17 }, // Slowbro
                new EncounterSlot4 { Species = 080, LevelMin = 37, LevelMax = 37 }, // Slowbro
                new EncounterSlot4 { Species = 080, LevelMin = 42, LevelMax = 42 }, // Slowbro
                new EncounterSlot4 { Species = 084, LevelMin = 15, LevelMax = 17 }, // Doduo
                new EncounterSlot4 { Species = 085, LevelMin = 42, LevelMax = 42 }, // Dodrio
                new EncounterSlot4 { Species = 098, LevelMin = 15, LevelMax = 17 }, // Krabby
                new EncounterSlot4 { Species = 099, LevelMin = 40, LevelMax = 40 }, // Kingler
                new EncounterSlot4 { Species = 179, LevelMin = 43, LevelMax = 43 }, // Mareep
                new EncounterSlot4 { Species = 304, LevelMin = 44, LevelMax = 45 }, // Aron
                new EncounterSlot4 { Species = 309, LevelMin = 42, LevelMax = 42 }, // Electrike
                new EncounterSlot4 { Species = 310, LevelMin = 37, LevelMax = 37 }, // Manectric
                new EncounterSlot4 { Species = 406, LevelMin = 40, LevelMax = 40 }, // Budew
                new EncounterSlot4 { Species = 443, LevelMin = 44, LevelMax = 44 }, // Gible
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_ROCKYBEACH_Surf = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Surf_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 060, LevelMin = 15, LevelMax = 16 }, // Poliwag
                new EncounterSlot4 { Species = 061, LevelMin = 16, LevelMax = 17 }, // Poliwhirl
                new EncounterSlot4 { Species = 129, LevelMin = 15, LevelMax = 16 }, // Magikarp
                new EncounterSlot4 { Species = 131, LevelMin = 15, LevelMax = 16 }, // Lapras
                new EncounterSlot4 { Species = 131, LevelMin = 36, LevelMax = 37 }, // Lapras
                new EncounterSlot4 { Species = 131, LevelMin = 41, LevelMax = 42 }, // Lapras
                new EncounterSlot4 { Species = 131, LevelMin = 46, LevelMax = 47 }, // Lapras
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_ROCKYBEACH_Old = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Old_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 098, LevelMin = 13, LevelMax = 15 }, // Krabby
                new EncounterSlot4 { Species = 098, LevelMin = 17, LevelMax = 17 }, // Krabby
                new EncounterSlot4 { Species = 098, LevelMin = 18, LevelMax = 18 }, // Krabby
                new EncounterSlot4 { Species = 118, LevelMin = 13, LevelMax = 15 }, // Goldeen
                new EncounterSlot4 { Species = 129, LevelMin = 12, LevelMax = 14 }, // Magikarp
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_ROCKYBEACH_Good = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Good_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 098, LevelMin = 22, LevelMax = 25 }, // Krabby
                new EncounterSlot4 { Species = 099, LevelMin = 26, LevelMax = 27 }, // Kingler
                new EncounterSlot4 { Species = 118, LevelMin = 22, LevelMax = 23 }, // Goldeen
                new EncounterSlot4 { Species = 129, LevelMin = 22, LevelMax = 23 }, // Magikarp
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_ROCKYBEACH_Super = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Super_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 099, LevelMin = 38, LevelMax = 39 }, // Kingler
                new EncounterSlot4 { Species = 118, LevelMin = 35, LevelMax = 38 }, // Goldeen
                new EncounterSlot4 { Species = 119, LevelMin = 35, LevelMax = 38 }, // Seaking
                new EncounterSlot4 { Species = 341, LevelMin = 46, LevelMax = 46 }, // Corphish
                new EncounterSlot4 { Species = 341, LevelMin = 48, LevelMax = 48 }, // Corphish
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_WASTELAND = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 022, LevelMin = 15, LevelMax = 17 }, // Fearow
                new EncounterSlot4 { Species = 055, LevelMin = 45, LevelMax = 45 }, // Golduck
                new EncounterSlot4 { Species = 066, LevelMin = 16, LevelMax = 17 }, // Machop
                new EncounterSlot4 { Species = 067, LevelMin = 17, LevelMax = 17 }, // Machoke
                new EncounterSlot4 { Species = 067, LevelMin = 40, LevelMax = 40 }, // Machoke
                new EncounterSlot4 { Species = 069, LevelMin = 41, LevelMax = 41 }, // Bellsprout
                new EncounterSlot4 { Species = 081, LevelMin = 15, LevelMax = 17 }, // Magnemite
                new EncounterSlot4 { Species = 095, LevelMin = 15, LevelMax = 17 }, // Onix
                new EncounterSlot4 { Species = 099, LevelMin = 48, LevelMax = 48 }, // Kingler
                new EncounterSlot4 { Species = 115, LevelMin = 15, LevelMax = 17 }, // Kangaskhan
                new EncounterSlot4 { Species = 286, LevelMin = 46, LevelMax = 46 }, // Breloom
                new EncounterSlot4 { Species = 308, LevelMin = 44, LevelMax = 44 }, // Medicham
                new EncounterSlot4 { Species = 310, LevelMin = 41, LevelMax = 41 }, // Manectric
                new EncounterSlot4 { Species = 314, LevelMin = 46, LevelMax = 46 }, // Illumise
                new EncounterSlot4 { Species = 338, LevelMin = 45, LevelMax = 46 }, // Solrock
                new EncounterSlot4 { Species = 451, LevelMin = 44, LevelMax = 45 }, // Skorupi
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_SAVANNAH = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 029, LevelMin = 15, LevelMax = 17 }, // Nidoran♀
                new EncounterSlot4 { Species = 030, LevelMin = 15, LevelMax = 17 }, // Nidorina
                new EncounterSlot4 { Species = 032, LevelMin = 15, LevelMax = 17 }, // Nidoran♂
                new EncounterSlot4 { Species = 033, LevelMin = 15, LevelMax = 17 }, // Nidorino
                new EncounterSlot4 { Species = 041, LevelMin = 15, LevelMax = 17 }, // Zubat
                new EncounterSlot4 { Species = 042, LevelMin = 17, LevelMax = 17 }, // Golbat
                new EncounterSlot4 { Species = 111, LevelMin = 17, LevelMax = 17 }, // Rhyhorn
                new EncounterSlot4 { Species = 111, LevelMin = 41, LevelMax = 41 }, // Rhyhorn
                new EncounterSlot4 { Species = 112, LevelMin = 44, LevelMax = 44 }, // Rhydon
                new EncounterSlot4 { Species = 128, LevelMin = 15, LevelMax = 17 }, // Tauros
                new EncounterSlot4 { Species = 128, LevelMin = 41, LevelMax = 41 }, // Tauros
                new EncounterSlot4 { Species = 228, LevelMin = 42, LevelMax = 42 }, // Houndour
                new EncounterSlot4 { Species = 263, LevelMin = 38, LevelMax = 38 }, // Zigzagoon
                new EncounterSlot4 { Species = 285, LevelMin = 45, LevelMax = 45 }, // Shroomish
                new EncounterSlot4 { Species = 298, LevelMin = 42, LevelMax = 42 }, // Azurill
                new EncounterSlot4 { Species = 324, LevelMin = 46, LevelMax = 47 }, // Torkoal
                new EncounterSlot4 { Species = 332, LevelMin = 42, LevelMax = 42 }, // Cacturne
                new EncounterSlot4 { Species = 404, LevelMin = 45, LevelMax = 46 }, // Luxio
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_WETLAND_Grass = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Grass_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 021, LevelMin = 15, LevelMax = 17 }, // Spearow
                new EncounterSlot4 { Species = 054, LevelMin = 15, LevelMax = 16 }, // Psyduck
                new EncounterSlot4 { Species = 055, LevelMin = 17, LevelMax = 17 }, // Golduck
                new EncounterSlot4 { Species = 055, LevelMin = 40, LevelMax = 40 }, // Golduck
                new EncounterSlot4 { Species = 083, LevelMin = 15, LevelMax = 17 }, // Farfetch'd
                new EncounterSlot4 { Species = 083, LevelMin = 41, LevelMax = 41 }, // Farfetch'd
                new EncounterSlot4 { Species = 084, LevelMin = 45, LevelMax = 45 }, // Doduo
                new EncounterSlot4 { Species = 132, LevelMin = 17, LevelMax = 17 }, // Ditto
                new EncounterSlot4 { Species = 132, LevelMin = 41, LevelMax = 41 }, // Ditto
                new EncounterSlot4 { Species = 161, LevelMin = 15, LevelMax = 17 }, // Sentret
                new EncounterSlot4 { Species = 162, LevelMin = 37, LevelMax = 37 }, // Furret
                new EncounterSlot4 { Species = 194, LevelMin = 15, LevelMax = 17 }, // Wooper
                new EncounterSlot4 { Species = 195, LevelMin = 16, LevelMax = 17 }, // Quagsire
                new EncounterSlot4 { Species = 271, LevelMin = 47, LevelMax = 47 }, // Lombre
                new EncounterSlot4 { Species = 283, LevelMin = 40, LevelMax = 40 }, // Surskit
                new EncounterSlot4 { Species = 372, LevelMin = 46, LevelMax = 46 }, // Shelgon
                new EncounterSlot4 { Species = 417, LevelMin = 43, LevelMax = 43 }, // Pachirisu
                new EncounterSlot4 { Species = 418, LevelMin = 44, LevelMax = 45 }, // Buizel
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_WETLAND_Surf = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Surf_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 054, LevelMin = 16, LevelMax = 17 }, // Psyduck
                new EncounterSlot4 { Species = 055, LevelMin = 37, LevelMax = 37 }, // Golduck
                new EncounterSlot4 { Species = 055, LevelMin = 45, LevelMax = 45 }, // Golduck
                new EncounterSlot4 { Species = 060, LevelMin = 15, LevelMax = 16 }, // Poliwag
                new EncounterSlot4 { Species = 195, LevelMin = 16, LevelMax = 17 }, // Quagsire
                new EncounterSlot4 { Species = 195, LevelMin = 37, LevelMax = 37 }, // Quagsire
                new EncounterSlot4 { Species = 194, LevelMin = 15, LevelMax = 16 }, // Wooper
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_WETLAND_Old = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Old_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 060, LevelMin = 12, LevelMax = 15 }, // Poliwag
                new EncounterSlot4 { Species = 061, LevelMin = 17, LevelMax = 18 }, // Poliwhirl
                new EncounterSlot4 { Species = 129, LevelMin = 12, LevelMax = 15 }, // Magikarp
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_WETLAND_Good = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Good_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 060, LevelMin = 22, LevelMax = 24 }, // Poliwag
                new EncounterSlot4 { Species = 061, LevelMin = 23, LevelMax = 25 }, // Poliwhirl
                new EncounterSlot4 { Species = 341, LevelMin = 26, LevelMax = 26 }, // Corphish
                new EncounterSlot4 { Species = 341, LevelMin = 28, LevelMax = 28 }, // Corphish
            },
        };

        private static readonly EncounterArea4HGSS SAFARIZONE_WETLAND_Super = new EncounterArea4HGSS
        {
            Location = 202,
            Type = SlotType.Super_Rod_Safari,
            Slots = new[]
            {
                new EncounterSlot4 { Species = 060, LevelMin = 35, LevelMax = 37 }, // Poliwag
                new EncounterSlot4 { Species = 061, LevelMin = 35, LevelMax = 37 }, // Poliwhirl
                new EncounterSlot4 { Species = 130, LevelMin = 44, LevelMax = 45 }, // Gyarados
                new EncounterSlot4 { Species = 130, LevelMin = 47, LevelMax = 48 }, // Gyarados
            },
        };

        public static readonly EncounterArea4HGSS[] SlotsHGSSAlt =
        {
            BCC_PreNational,
            BCC_PostTuesday,
            BCC_PostThursday,
            BCC_PostSaturday,

            SAFARIZONE_PEAK,
            SAFARIZONE_DESERT,
            SAFARIZONE_PLAINS,
            SAFARIZONE_MEADOW_Grass,
            SAFARIZONE_MEADOW_Surf,
            SAFARIZONE_MEADOW_Old,
            SAFARIZONE_MEADOW_Good,
            SAFARIZONE_MEADOW_Super,
            SAFARIZONE_FOREST,
            SAFARIZONE_SWAMP_Grass,
            SAFARIZONE_SWAMP_Surf,
            SAFARIZONE_SWAMP_Old,
            SAFARIZONE_SWAMP_Good,
            SAFARIZONE_SWAMP_Super,
            SAFARIZONE_MARSHLAND_Grass,
            SAFARIZONE_MARSHLAND_Surf,
            SAFARIZONE_MARSHLAND_Old,
            SAFARIZONE_MARSHLAND_Good,
            SAFARIZONE_MARSHLAND_Super,
            SAFARIZONE_MOUNTAIN,
            SAFARIZONE_ROCKYBEACH_Grass,
            SAFARIZONE_ROCKYBEACH_Surf,
            SAFARIZONE_ROCKYBEACH_Old,
            SAFARIZONE_ROCKYBEACH_Good,
            SAFARIZONE_ROCKYBEACH_Super,
            SAFARIZONE_WASTELAND,
            SAFARIZONE_SAVANNAH,
            SAFARIZONE_WETLAND_Grass,
            SAFARIZONE_WETLAND_Surf,
            SAFARIZONE_WETLAND_Old,
            SAFARIZONE_WETLAND_Good,
            SAFARIZONE_WETLAND_Super
        };
    }
}