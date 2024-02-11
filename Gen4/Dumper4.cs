using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;
using static PKHeX.EncounterSlotDumper.SlotType4;
using static PKHeX.EncounterSlotDumper.EncounterType;

namespace PKHeX.EncounterSlotDumper;

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

        var HG_Headbutt_Slots = EncounterArea4HGSS.GetArray4HGSS_Headbutt(BinLinker.Unpack(hb_hg, "hg"));
        var SS_Headbutt_Slots = EncounterArea4HGSS.GetArray4HGSS_Headbutt(BinLinker.Unpack(hb_ss, "ss"));

        var D_HoneyTrees_Slots = SlotsD_HoneyTree.Split(HoneyTreesLocation);
        var P_HoneyTrees_Slots = SlotsP_HoneyTree.Split(HoneyTreesLocation);
        var Pt_HoneyTrees_Slots = SlotsPt_HoneyTree.Split(HoneyTreesLocation);

        MarkG4SwarmSlots(HG_Slots, SlotsHG_Swarm);
        MarkG4SwarmSlots(SS_Slots, SlotsSS_Swarm);

        MarkEncounterTypeData(D_Slots, P_Slots, Pt_Slots, HG_Slots, SS_Slots);

        var DP_Feebas = GetFeebasArea(D_Slots[55], D_Slots[56], D_Slots[57]);
        var Pt_Feebas = GetFeebasArea(Pt_Slots[55], Pt_Slots[56], Pt_Slots[57]);
        EncounterArea4DPPt[] SlotsD  = [..D_Slots,  ..D_HoneyTrees_Slots,  ..DP_Feebas];
        EncounterArea4DPPt[] SlotsP  = [..P_Slots,  ..P_HoneyTrees_Slots,  ..DP_Feebas];
        EncounterArea4DPPt[] SlotsPt = [..Pt_Slots, ..Pt_HoneyTrees_Slots, ..Pt_Feebas];

        var alt = Encounters4Extra.SlotsHGSSAlt;
        var safari = Dumper4Safari.GetSafariAreaSets();
        EncounterArea4HGSS[] SlotsHG = [..HG_Slots, ..HG_Headbutt_Slots, ..alt, ..safari];
        EncounterArea4HGSS[] SlotsSS = [..SS_Slots, ..SS_Headbutt_Slots, ..alt, ..safari];

        MarkDPPtEncounterTypeSlots(SlotsD);
        MarkDPPtEncounterTypeSlots(SlotsP);
        MarkDPPtEncounterTypeSlots(SlotsPt);
        MarkHGSSEncounterTypeSlots(SlotsHG);
        MarkHGSSEncounterTypeSlots(SlotsSS);

        // Remove inaccessible area slots
        // Johto Route 45 surfing encounter. Unreachable Water tiles.
        SlotsHG = SlotsHG.Where(z => z.Location != 193 || z.Type != Surf).ToArray();
        SlotsSS = SlotsSS.Where(z => z.Location != 193 || z.Type != Surf).ToArray();

        Write(SlotsD, "encounter_d.pkl", "da");
        Write(SlotsP, "encounter_p.pkl", "pe");
        Write(SlotsPt, "encounter_pt.pkl", "pt");
        Write(SlotsHG, "encounter_hg.pkl", "hg");
        Write(SlotsSS, "encounter_ss.pkl", "ss");

        if (Dumper4Safari.ExportParse)
            File.WriteAllLines("safari.txt", Dumper4Safari.Parse);
    }

    public static void Write(IEnumerable<EncounterArea4> area, string name, string ident = "g4")
    {
        area = area.OrderBy(z => z.Location).ThenBy(z => z.Type).ThenBy(z => z.TypeEncounter);
        var serialized = area.Select(Write).ToArray();
        List<byte[]> unique = [];
        foreach (var a in serialized)
        {
            if (unique.Any(z => z.SequenceEqual(a)))
                continue;
            unique.Add(a);
        }

        var packed = BinLinker.Pack([.. unique], ident);
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

        foreach (var slot in area.Slots)
            WriteSlot(bw, slot);

        return ms.ToArray();
    }

    private static void WriteSlot(BinaryWriter bw, EncounterSlot4 slot)
    {
        bw.Write(slot.Species);
        bw.Write(slot.Form);
        bw.Write(slot.SlotNumber);
        bw.Write(slot.LevelMin);
        bw.Write(slot.LevelMax);
        bw.Write(slot.MagnetPullIndex);
        bw.Write(slot.MagnetPullCount);
        bw.Write(slot.StaticIndex);
        bw.Write(slot.StaticCount);
    }

    private static readonly EncounterArea4DPPt SlotsPt_HoneyTree = new()
    {
        Location = 0,
        Rate = 0,
        Type = HoneyTree,
        Slots =
        [
            new EncounterSlot4 {Species = 190, LevelMin = 5, LevelMax = 15}, // Aipom
            new EncounterSlot4 {Species = 214, LevelMin = 5, LevelMax = 15}, // Heracross
            new EncounterSlot4 {Species = 265, LevelMin = 5, LevelMax = 15}, // Wurmple
            new EncounterSlot4 {Species = 412, LevelMin = 5, LevelMax = 15, Form = 0}, // Burmy Plant Cloak
            new EncounterSlot4 {Species = 415, LevelMin = 5, LevelMax = 15}, // Combee
            new EncounterSlot4 {Species = 420, LevelMin = 5, LevelMax = 15}, // Cheruby
            new EncounterSlot4 {Species = 446, LevelMin = 5, LevelMax = 15} // Munchlax
        ],
    };

    private static readonly EncounterArea4DPPt SlotsD_HoneyTree = new()
    {
        Location = 0,
        Rate = 0,
        Type = HoneyTree,
        Slots =
        [
            .. SlotsPt_HoneyTree.Slots,
            .. new[] { new EncounterSlot4 {Species = 266, LevelMin = 5, LevelMax = 15} }, // Silcoon
        ],
    };

    private static readonly EncounterArea4DPPt SlotsP_HoneyTree = new()
    {
        Location = 0,
        Rate = 0,
        Type = HoneyTree,
        Slots =
        [
            .. SlotsPt_HoneyTree.Slots,
            .. new[] { new EncounterSlot4 {Species = 268, LevelMin = 5, LevelMax = 15} }, // Cascoon
        ],
    };

    private static EncounterArea4DPPt[] GetFeebasArea(params EncounterArea4DPPt[] areas)
    {
#if DEBUG
        Debug.Assert(areas.Last().Location == 50); // Mt Coronet
        Debug.Assert(areas.Last().Slots.Last().Species == (int)Species.Whiscash);
        Debug.Assert(areas.Last().Slots[0].Species == (int)Species.Gyarados);

        Debug.Assert(areas.Length == 3);
        Debug.Assert(areas[0].Type == Old_Rod);
        Debug.Assert(areas[1].Type == Good_Rod);
        Debug.Assert(areas[2].Type == Super_Rod);

        foreach (var area in areas)
            area.Rate = byte.MaxValue; // Tag for Feebas handling.
#endif
        var result = new EncounterArea4DPPt[3];
        for (int i = 0; i < result.Length; i++)
        {
            // Feebas replaces the encounter slot species.
            var temp = areas[i];
            var tSlots = temp.Slots;

            var slots = new EncounterSlot4[tSlots.Length];
            for (var j = 0; j < slots.Length; j++)
                slots[j] = new() { Species = (int)Species.Feebas, LevelMin = 10, LevelMax = 20, SlotNumber = (byte)i };

            result[i] = temp with { Slots = slots };
        }

        return result;
    }

    private static ReadOnlySpan<byte> Shellos_EastSeaLocation_DP =>
    [
        28, // Route 213
        39, // Route 224
    ];

    private static ReadOnlySpan<byte> Shellos_EastSeaLocation_Pt =>
    [
        11, // Pastoria City
        27, // Route 212
        28, // Route 213
    ];

    private static ReadOnlySpan<byte> Gastrodon_EastSeaLocation_DP =>
    [
        37, // Route 222
        39, // Route 224
        45, // Route 230
    ];

    private static ReadOnlySpan<byte> Gastrodon_EastSeaLocation_Pt =>
    [
        11, // Pastoria City
        27, // Route 212
        28, // Route 213
        39, // Route 224
        45, // Route 230
    ];

    private static ReadOnlySpan<byte> HoneyTreesLocation =>
    [
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
    ];

    private static ReadOnlySpan<byte> MetLocationSolaceonRuins => [53];
    private static ReadOnlySpan<byte> MetLocationRuinsOfAlph => [209];

    private static void MarkEncounterTypeData(EncounterArea4DPPt[] D_Slots, EncounterArea4DPPt[] P_Slots,
        EncounterArea4DPPt[] Pt_Slots, 
        EncounterArea4HGSS[] HG_Slots, EncounterArea4HGSS[] SS_Slots)
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

        const byte Route209 = 24;
        const byte StarkMountain = 84;
        const byte MtCoronet = 50;
        const byte RuinsOfAlph = 209;
        const byte MtSilver = 219;
        const byte Cianwood = 130;
        MarkDPPtEncounterTypeSlots_MultipleTypes(D_Slots, Route209, Building_EnigmaStone, 0); // Exterior slots (Starly); not Lost Tower tables.
        MarkDPPtEncounterTypeSlots_MultipleTypes(P_Slots, Route209, Building_EnigmaStone, 0); // Exterior slots (Starly); not Lost Tower tables.
        MarkDPPtEncounterTypeSlots_MultipleTypes(Pt_Slots, Route209, Building_EnigmaStone, 0); // Exterior slots (Starly); not Lost Tower tables.
        MarkDPPtEncounterTypeSlots_MultipleTypes(D_Slots, StarkMountain, Cave_HallOfOrigin, 0); // Stark Mountain Camerupt
        MarkDPPtEncounterTypeSlots_MultipleTypes(P_Slots, StarkMountain, Cave_HallOfOrigin, 0); // Stark Mountain Camerupt
        MarkDPPtEncounterTypeSlots_MultipleTypes(Pt_Slots, StarkMountain, Cave_HallOfOrigin, 0); // Stark Mountain Camerupt
        MarkDPPtEncounterTypeSlots_MultipleTypes(D_Slots, MtCoronet, Cave_HallOfOrigin, DPPt_MtCoronetExteriorEncounters); // Snover land slots
        MarkDPPtEncounterTypeSlots_MultipleTypes(P_Slots, MtCoronet, Cave_HallOfOrigin, DPPt_MtCoronetExteriorEncounters); // Snover land slots
        MarkDPPtEncounterTypeSlots_MultipleTypes(Pt_Slots, MtCoronet, Cave_HallOfOrigin, DPPt_MtCoronetExteriorEncounters); // Snover land slots
        MarkHGSSEncounterTypeSlots_MultipleTypes(HG_Slots, RuinsOfAlph, Cave_HallOfOrigin, 0, 1, 2, 3, 4, 5); // Alph Exterior (not Unown)
        MarkHGSSEncounterTypeSlots_MultipleTypes(SS_Slots, RuinsOfAlph, Cave_HallOfOrigin, 0, 1, 2, 3, 4, 5); // Alph Exterior (not Unown)
        MarkHGSSEncounterTypeSlots_MultipleTypes(HG_Slots, MtSilver, Cave_HallOfOrigin, HGSS_MtSilverCaveExteriorEncounters); // Exterior
        MarkHGSSEncounterTypeSlots_MultipleTypes(SS_Slots, MtSilver, Cave_HallOfOrigin, HGSS_MtSilverCaveExteriorEncounters); // Exterior

        MarkHGSSEncounterTypeSlots_MultipleTypes(HG_Slots, Cianwood, RockSmash);
        MarkHGSSEncounterTypeSlots_MultipleTypes(SS_Slots, Cianwood, RockSmash);

        MarkSpecific(HG_Slots, RuinsOfAlph, Rock_Smash, DialgaPalkia);
        MarkSpecific(SS_Slots, RuinsOfAlph, Rock_Smash, DialgaPalkia);
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
                var c0 = area.Slots[index] with
                {
                    Species = swarm.Species,
                    Form = 0,
                };
                if (index != c0.SlotNumber)
                    throw new Exception();
                extra.Add(c0);

                // edge case, Mawile is only swarm subject to magnet pull (no other steel types in area)
                if (swarm.Species == (int)Species.Mawile)
                {
                    c0.MagnetPullIndex = c0.SlotNumber;
                    c0.MagnetPullCount = 2;
                }
            }

            if (extra.Count == 0)
                throw new Exception();

            area.Slots = [.. area.Slots, ..extra];
        }
    }

    private static int[] GetSwarmSlotIndexes(SlotType4 type)
    {
        return type switch
        {
            // Grass Swarm slots replace slots 0 and 1 from encounters data
            Grass => [0, 1],
            // for surfing only replace slots 0 from encounters data
            Surf => [0],
            Old_Rod => [2],
            Good_Rod => [0, 2, 3],
            Super_Rod => [0, 1, 2, 3, 4], // all
            _ => throw new Exception()
        };
    }

    // Gen 4 raw encounter data does not contain info for alt slots
    // Shellos and Gastrodon East Sea form should be modified
    private static void MarkG4AltFormSlots(IEnumerable<EncounterArea4> areas, [ConstantExpected] ushort Species, [ConstantExpected] byte form, ReadOnlySpan<byte> locations)
    {
        foreach (var area in areas)
        {
            if (!locations.Contains(area.Location)) 
                continue;
            foreach (var slot in area.Slots)
            {
                if (slot.Species == Species) 
                    slot.Form = form;
            }
        }
    }

    private static EncounterType GetEncounterTypeBySlotDPPt(SlotType4 type, EncounterType grass) => type switch
    {
        Grass => grass,
        Surf => Surfing_Fishing,
        Old_Rod => Surfing_Fishing,
        Good_Rod => Surfing_Fishing,
        Super_Rod => Surfing_Fishing,
        Safari_Surf => Surfing_Fishing,
        Safari_Old_Rod => Surfing_Fishing,
        Safari_Good_Rod => Surfing_Fishing,
        Safari_Super_Rod => Surfing_Fishing,
        Safari_Grass => MarshSafari,
        HoneyTree => None,
        _ => None
    };

    private static EncounterType GetEncounterTypeBySlotHGSS(SlotType4 type, EncounterType grass, EncounterType headbutt) => type switch
    {
        Grass => grass,
        Surf => Surfing_Fishing,
        Old_Rod => Surfing_Fishing,
        Good_Rod => Surfing_Fishing,
        Super_Rod => Surfing_Fishing,

        Rock_Smash when grass == RockSmash => RockSmash | Building_EnigmaStone,
        Rock_Smash when headbutt == Building_EnigmaStone => headbutt,
        Rock_Smash when grass == Cave_HallOfOrigin => grass,
        Rock_Smash => None,

        // not sure on if "None" should always be allowed, but this is so uncommon it shouldn't matter (gen7 doesn't keep this value anyway).
        HeadbuttSpecial => headbutt | None,
        Headbutt => headbutt | None,
        BugContest => grass,

        // HGSS Safari encounters have normal water/grass encounter type, not safari encounter type
        Safari_Grass => grass,
        Safari_Surf => Surfing_Fishing,
        Safari_Old_Rod => Surfing_Fishing,
        Safari_Good_Rod => Surfing_Fishing,
        Safari_Super_Rod => Surfing_Fishing,

        _ => None
    };

    private static void MarkDPPtEncounterTypeSlots_MultipleTypes(EncounterArea4DPPt[] areas, [ConstantExpected] byte location,
        EncounterType normalEncounterType, ReadOnlySpan<byte> tallGrassAreaIndexes)
    {
        byte numfile = 0;
        var iterate = areas.Where(x => x.Location == location);
        foreach (var area in iterate)
        {
            var GrassType = tallGrassAreaIndexes.Contains(numfile) ? TallGrass : normalEncounterType;
            area.TypeEncounter = GetEncounterTypeBySlotDPPt(area.Type, GrassType);
            numfile++;
        }
    }

    private static void MarkDPPtEncounterTypeSlots_MultipleTypes(EncounterArea4DPPt[] areas, [ConstantExpected] byte location, 
        EncounterType normalEncounterType, params byte[] tallGrassAreaIndexes)
    {
        ReadOnlySpan<byte> span = tallGrassAreaIndexes;
        MarkDPPtEncounterTypeSlots_MultipleTypes(areas, location, normalEncounterType, span);
    }

    private static void MarkHGSSEncounterTypeSlots_MultipleTypes(EncounterArea4HGSS[] Areas, [ConstantExpected] byte location,
        EncounterType normalEncounterType, ReadOnlySpan<byte> tallGrassAreaIndexes)
    {
        // Area with two different encounter type for grass encounters
        // SpecialEncounterFile is tall grass encounter type, the other files have the normal encounter type for this location
        var HeadbuttType = GetHeadbuttEncounterType(location);
        byte numfile = 0;
        var iterate = Areas.Where(x => x.Location == location);
        foreach (var area in iterate)
        {
            var GrassType = tallGrassAreaIndexes.Contains(numfile) ? TallGrass : normalEncounterType;
            area.TypeEncounter = GetEncounterTypeBySlotHGSS(area.Type, GrassType, HeadbuttType);
            numfile++;
        }
    }

    private static void MarkHGSSEncounterTypeSlots_MultipleTypes(EncounterArea4HGSS[] Areas, [ConstantExpected] byte Location,
        EncounterType normalEncounterType, params byte[] tallGrassAreaIndexes)
    {
        ReadOnlySpan<byte> span = tallGrassAreaIndexes;
        MarkHGSSEncounterTypeSlots_MultipleTypes(Areas, Location, normalEncounterType, span);
    }

    private static void MarkSpecific(EncounterArea4HGSS[] Areas, byte Location, SlotType4 t, EncounterType val)
    {
        var areas = Areas.Where(x => x.Location == Location && x.Type == t);
        foreach (var area in areas)
            area.TypeEncounter = val;
    }

    private static void MarkDPPtEncounterTypeSlots(EncounterArea4DPPt[] areas)
    {
        foreach (var area in areas)
        {
            if (DPPt_MixInteriorExteriorLocations.Contains(area.Location))
                continue;

            var grassType = GetGrassType(area.Location);

            static EncounterType GetGrassType(byte location)
            {
                if (location == 70) // Old Chateau
                    return Building_EnigmaStone;
                if (DPPt_CaveLocations.Contains(location))
                    return Cave_HallOfOrigin;
                return TallGrass;
            }

            if (area.TypeEncounter == None) // not defined yet
            {
                if (area.Location == 52) // Great Marsh
                    area.TypeEncounter = area.Type == Grass ? MarshSafari : Surfing_Fishing;
                else
                    area.TypeEncounter = GetEncounterTypeBySlotDPPt(area.Type, grassType);
            }
        }
    }

    private static EncounterType GetHeadbuttEncounterType(byte location)
    {
        if (location == 195) // Route 47 -- one tree accessible via Water tile
            return DialgaPalkia | TallGrass | Surfing_Fishing;
        if (location == 196) // Route 48
            return DialgaPalkia | TallGrass;

        // Routes with trees adjacent to water tiles
        var allowSurf = HGSS_SurfingHeadbutt_Locations.Contains(location);

        // Cities
        if (HGSS_CityLocations.Contains(location))
        {
            return allowSurf
                ? Building_EnigmaStone | Surfing_Fishing
                : Building_EnigmaStone;
        }

        // Caves with no exterior zones
        if (!HGSS_MixInteriorExteriorLocations.Contains(location) && HGSS_CaveLocations.Contains(location))
        {
            return allowSurf
                ? Cave_HallOfOrigin | Surfing_Fishing
                : Cave_HallOfOrigin;
        }

        // Routes and exterior areas
        // Routes with trees adjacent to grass tiles
        var allowGrass = HGSS_GrassHeadbutt_Locations.Contains(location);
        if (allowGrass)
        {
            return allowSurf
                ? TallGrass | Surfing_Fishing
                : TallGrass;
        }

        return allowSurf
            ? Surfing_Fishing
            : None;
    }

    public static void MarkHGSSEncounterTypeSlots(IEnumerable<EncounterArea4> areas)
    {
        foreach (var area in areas)
        {
            if (HGSS_MixInteriorExteriorLocations.Contains(area.Location))
                continue;
            var GrassType = HGSS_CaveLocations.Contains(area.Location) ? Cave_HallOfOrigin : TallGrass;
            var HeadbuttType = GetHeadbuttEncounterType(area.Location);

            if (area.TypeEncounter == None) // not defined yet
                area.TypeEncounter = GetEncounterTypeBySlotHGSS(area.Type, GrassType, HeadbuttType);
        }
    }

    #region Encounter Types
    private static ReadOnlySpan<byte> DPPt_CaveLocations =>
    [
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
    ];

    private static ReadOnlySpan<byte> DPPt_MixInteriorExteriorLocations =>
    [
        24, // Route 209 (Lost Tower)
        50, // Mt Coronet
        84, // Stark Mountain
    ];

    private static ReadOnlySpan<byte> DPPt_MtCoronetExteriorEncounters =>
    [
        7, 8
    ];

    /// <summary>
    /// Locations with headbutt trees accessible from Cave tiles
    /// </summary>
    private static ReadOnlySpan<byte> HGSS_CaveLocations =>
    [
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
    ];

    /// <summary>
    /// Locations with headbutt trees accessible from city tiles
    /// </summary>
    private static ReadOnlySpan<byte> HGSS_CityLocations =>
    [
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
    ];

    /// <summary>
    /// Locations with headbutt trees accessible from water tiles
    /// </summary>
    private static ReadOnlySpan<byte> HGSS_SurfingHeadbutt_Locations =>
    [
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
        195, // Route 47 -- One tree at the very top of the waterfall.
        214, // Ilex Forest
    ];

    /// <summary>
    /// Locations with headbutt trees accessible from tall grass tiles
    /// </summary>
    private static ReadOnlySpan<byte> HGSS_GrassHeadbutt_Locations =>
    [
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
    ];

    private static ReadOnlySpan<byte> HGSS_MtSilverCaveExteriorEncounters =>
    [
        5, 10
    ];

    private static ReadOnlySpan<byte> HGSS_MixInteriorExteriorLocations =>
    [
        209, // Ruins of Alph
        219, // Mt. Silver Cave
    ];

    private record SwarmDef(short Location, ushort Species, SlotType4 Type, byte TableIndex = 0)
    {
        public override string ToString() => $"{Location}{(TableIndex == 0 ? "" : $"({TableIndex})")} - {(Species) Species}";
    }

    private static readonly SwarmDef[] SlotsHGSS_Swarm =
    [
        new(143, 278, Surf ), // Wingull @ Vermillion City
        new(149, 261, Grass), // Poochyena @ Route 1
        new(161, 113, Grass), // Chansey @ Route 13
        new(167, 366, Surf ), // Clamperl @ Route 19
        new(173, 427, Grass), // Buneary @ Route 25
        new(175, 370, Surf ), // Luvdisc @ Route 27
        new(182, 280, Grass), // Ralts @ Route 34
        new(183, 193, Grass), // Yanma @ Route 35
        new(186, 209, Grass), // Snubbull @ Route 38
        new(193, 333, Grass), // Swablu @ Route 45
        new(195, 132, Grass), // Ditto @ Route 47
        new(216, 183, Grass), // Marill @ Mt. Mortar
        new(220, 206, Grass, 1), // Dunsparce @ Dark Cave (Route 31 side; the r45 does not have Dunsparce swarm)
        new(224, 401, Grass), // Kricketot @ Viridian Forest

        new(128, 340, Old_Rod),   // Whiscash @ Violet City
        new(128, 340, Good_Rod),  // Whiscash @ Violet City
        new(128, 340, Super_Rod), // Whiscash @ Violet City

        new(160, 369, Old_Rod),   // Relicanth @ Route 12
        new(160, 369, Good_Rod),  // Relicanth @ Route 12
        new(160, 369, Super_Rod), // Relicanth @ Route 12

        new(180, 211, Old_Rod),   // Qwilfish @ Route 32
        new(180, 211, Good_Rod),  // Qwilfish @ Route 32
        new(180, 211, Super_Rod), // Qwilfish @ Route 32

        new(192, 223, Old_Rod),   // Remoraid @ Route 44
        new(192, 223, Good_Rod),  // Remoraid @ Route 44
        new(192, 223, Super_Rod), // Remoraid @ Route 44
    ];

    private static readonly SwarmDef[] SlotsHG_Swarm =
    [
        .. SlotsHGSS_Swarm,
        .. new[] {
            new SwarmDef(151, 343, Grass), // Baltoy @ Route 3
            new SwarmDef(157, 302, Grass), // Sableye @ Route 9
        },
    ];

    private static readonly SwarmDef[] SlotsSS_Swarm =
    [
        .. SlotsHGSS_Swarm,
        .. new[] {
            new SwarmDef(151, 316, Grass), // Gulpin @ Route 3
            new SwarmDef(157, 303, Grass), // Mawile @ Route 9
        },
    ];
    #endregion
}

public static class Encounters4Extra
{
    private const byte RateBCC = 25; // Assumed same as the underlying area (National Park, grass)

    private static readonly EncounterArea4HGSS BCC_PreNational = new()
    {
        Location = 207, // National Park Catching Contest
        Type = BugContest,
        Rate = RateBCC,
        Slots =
        [
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
        ]
    };

    private static readonly EncounterArea4HGSS BCC_PostTuesday = new()
    {
        Location = 207, // National Park Catching Contest
        Type = BugContest,
        Rate = RateBCC,
        Slots =
        [
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
        ]
    };

    private static readonly EncounterArea4HGSS BCC_PostThursday = new()
    {
        Location = 207, // National Park Catching Contest
        Type = BugContest,
        Rate = RateBCC,
        Slots =
        [
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
        ]
    };

    private static readonly EncounterArea4HGSS BCC_PostSaturday = new()
    {
        Location = 207, // National Park Catching Contest
        Type = BugContest,
        Rate = RateBCC,
        Slots =
        [
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
        ]
    };

    public static readonly EncounterArea4HGSS[] SlotsHGSSAlt =
    [
        BCC_PreNational,
        BCC_PostTuesday,
        BCC_PostThursday,
        BCC_PostSaturday,
    ];
}
