using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;
using static PKHeX.EncounterSlotDumper.SlotType5;

namespace PKHeX.EncounterSlotDumper;

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
            return EncounterArea5.GetArray(data);
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

        EncounterArea5[] SlotsB  = [..BSlots, ..SlotsB_Swarm];
        EncounterArea5[] SlotsW  = [..WSlots, ..SlotsW_Swarm, ..WhiteForestSlots];
        EncounterArea5[] SlotsB2 = [..B2Slots, ..SlotsB2_Swarm, SlotsB2_HiddenGrotto];
        EncounterArea5[] SlotsW2 = [..W2Slots, ..SlotsW2_Swarm, SlotsW2_HiddenGrotto];

        Write(SlotsB,  "encounter_b.pkl" , "51");
        Write(SlotsW , "encounter_w.pkl" , "51");
        Write(SlotsB2, "encounter_b2.pkl", "52");
        Write(SlotsW2, "encounter_w2.pkl", "52");
    }

    private static void MarkBWSwarmSlots(EncounterArea5[] Areas)
    {
        foreach (var area in Areas)
        {
            area.Type = Swarm;
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
            area.Type = Swarm;
            foreach (var s in area.Slots)
            {
                s.LevelMin = 40;
                s.LevelMax = 55;
            }
        }
    }

    private static void MarkG5HiddenGrottoSlots(EncounterArea5 Areas) => Areas.Type = HiddenGrotto;

    private static void MarkG5Slots(ref EncounterArea5[] Areas)
    {
        List<EncounterArea5> areas = [];
        foreach (var area in Areas)
        {
            int ctr = 0;
            do
            {
                areas.Add(new EncounterArea5 { Location = area.Location, Type = Grass, Slots = area.Slots.Skip(ctr).Take(12).ToArray() }); // Single
                ctr += 12;
                areas.Add(new EncounterArea5 { Location = area.Location, Type = Grass, Slots = area.Slots.Skip(ctr).Take(12).ToArray() }); // Double
                ctr += 12;
                areas.Add(new EncounterArea5 { Location = area.Location, Type = Grass, Slots = area.Slots.Skip(ctr).Take(12).ToArray() }); // Shaking
                ctr += 12;

                areas.Add(new EncounterArea5 { Location = area.Location, Type = Surf, Slots = area.Slots.Skip(ctr).Take(5).ToArray() }); // Surf
                ctr += 5;
                areas.Add(new EncounterArea5 { Location = area.Location, Type = Surf, Slots = area.Slots.Skip(ctr).Take(5).ToArray() }); // Surf Spot
                ctr += 5;
                areas.Add(new EncounterArea5 { Location = area.Location, Type = Super_Rod, Slots = area.Slots.Skip(ctr).Take(5).ToArray() }); // Fish
                ctr += 5;
                areas.Add(new EncounterArea5 { Location = area.Location, Type = Super_Rod, Slots = area.Slots.Skip(ctr).Take(5).ToArray() }); // Fish Spot
                ctr += 5;
            } while (ctr != area.Slots.Length);
            area.Slots = area.Slots.Where(slot => slot.Species != 0).ToArray();
        }

        Areas = [.. areas];
    }

    public static void Write(IEnumerable<EncounterArea5> area, string name, string ident = "g5")
    {
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

    public static byte[] Write(EncounterArea5 area)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write((ushort)area.Location);
        bw.Write((byte)area.Type);
        bw.Write((byte)0);

        foreach (var slot in area.Slots)
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

    private static ReadOnlySpan<ushort> WhiteForest_GrassSpecies =>
    [
        016, 029, 032, 043, 063, 066, 069, 081, 092, 111,
        137, 175, 179, 187, 239, 240, 265, 270, 273, 280,
        287, 293, 298, 304, 328, 371, 396, 403, 406, 440
    ];

    private static ReadOnlySpan<ushort> WhiteForest_SurfSpecies =>
    [
        194, 270, 283, 341,
    ];

    private static readonly EncounterArea5[] WhiteForestSlots =
    [
        EncounterArea5.GetSimpleEncounterArea(WhiteForest_GrassSpecies, [ 5, 5 ], 51, Grass),
        EncounterArea5.GetSimpleEncounterArea(WhiteForest_SurfSpecies, [ 5, 5 ], 51, Surf),
    ];
    private static readonly EncounterArea5[] SlotsBW_Swarm =
    [
        // Level Range and Slot Type will be marked later
        new() { Location = 014, Slots = [new() { Species = 083 }], }, // Farfetch'd @ Route 1
        new() { Location = 015, Slots = [new() { Species = 360 }], }, // Wynaut @ Route 2
        new() { Location = 017, Slots = [new() { Species = 449 }], }, // Hippopotas @ Route 4
        new() { Location = 018, Slots = [new() { Species = 235 }], }, // Smeargle @ Route 5
        new() { Location = 020, Slots = [new() { Species = 161 }], }, // Sentret @ Route 7
        new() { Location = 021, Slots = [new() { Species = 453 }], }, // Croagunk @ Route 8
        new() { Location = 023, Slots = [new() { Species = 236 }], }, // Tyrogue @ Route 10
        new() { Location = 025, Slots = [new() { Species = 084 }], }, // Doduo @ Route 12
        new() { Location = 026, Slots = [new() { Species = 353 }], }, // Shuppet @ Route 13
        new() { Location = 027, Slots = [new() { Species = 193 }], }, // Yanma @ Route 14
        new() { Location = 028, Slots = [new() { Species = 056 }], }, // Mankey @ Route 15
        new() { Location = 029, Slots = [new() { Species = 204 }], }, // Pineco @ Route 16
        new() { Location = 031, Slots = [new() { Species = 102 }], }, // Exeggcute @ Route 18
    ];

    private static readonly EncounterArea5[] SlotsB_Swarm = [.. SlotsBW_Swarm,
        new() { Location = 016, Slots = [new() { Species = 313 }], }, // Volbeat @ Route 3
        new() { Location = 019, Slots = [new() { Species = 311 }], }, // Plusle @ Route 6
        new() { Location = 022, Slots = [new() { Species = 228 }], }, // Houndour @ Route 9
        new() { Location = 024, Slots = [new() { Species = 285 }], }, // Shroomish @ Route 11
    ];

    private static readonly EncounterArea5[] SlotsW_Swarm = [.. SlotsBW_Swarm,
        new() { Location = 016, Slots = [new() { Species = 314 }], }, // Illumise @ Route 3
        new() { Location = 019, Slots = [new() { Species = 312 }], }, // Minun @ Route 6
        new() { Location = 022, Slots = [new() { Species = 261 }], }, // Poochyena @ Route 9
        new() { Location = 024, Slots = [new() { Species = 046 }], }, // Paras @ Route 11
    ];

    private static readonly EncounterArea5[] SlotsB2W2_Swarm =
    [
        // Level Range and Slot Type will be marked later
        new() { Location = 014, Slots = [new() { Species = 083 }], }, // Farfetch'd @ Route 1
        new() { Location = 018, Slots = [new() { Species = 177 }], }, // Natu @ Route 5
        new() { Location = 020, Slots = [new() { Species = 162 }], }, // Furret @ Route 7
        new() { Location = 021, Slots = [new() { Species = 195 }], }, // Quagsire @ Route 8
        new() { Location = 022, Slots = [new() { Species = 317 }], }, // Swalot @ Route 9
        new() { Location = 024, Slots = [new() { Species = 284 }], }, // Masquerain @ Route 11
        new() { Location = 025, Slots = [new() { Species = 084 }], }, // Doduo @ Route 12
        new() { Location = 026, Slots = [new() { Species = 277 }], }, // Swellow @ Route 13
        new() { Location = 028, Slots = [new() { Species = 022 }], }, // Fearow @ Route 15
        new() { Location = 029, Slots = [new() { Species = 204 }], }, // Pineco @ Route 16
        new() { Location = 031, Slots = [new() { Species = 187 }], }, // Hoppip @ Route 18
        new() { Location = 032, Slots = [new() { Species = 097 }], }, // Hypno @ Dreamyard
        new() { Location = 034, Slots = [new() { Species = 450 }], }, // Hippowdon @ Desert Resort
        new() { Location = 070, Slots = [new() { Species = 079 }], }, // Slowpoke @ Abundant shrine
        new() { Location = 132, Slots = [new() { Species = 332 }], }, // Cacturne @ Reaversal Mountian
    ];

    private static readonly EncounterArea5[] SlotsB2_Swarm = [.. SlotsB2W2_Swarm,
        new() { Location = 016, Slots = [new() { Species = 313 }], }, // Volbeat @ Route 3
        new() { Location = 019, Slots = [new() { Species = 311 }], }, // Plusle @ Route 6
        new() { Location = 125, Slots = [new() { Species = 185 }], }, // Sudowoodo @ Route 20
        new() { Location = 127, Slots = [new() { Species = 168 }], }, // Ariados @ Route 22
    ];

    private static readonly EncounterArea5[] SlotsW2_Swarm = [.. SlotsB2W2_Swarm,
        new() { Location = 016, Slots = [new() { Species = 314 }], }, // Illumise @ Route 3
        new() { Location = 019, Slots = [new() { Species = 312 }], }, // Minun @ Route 6
        new() { Location = 125, Slots = [new() { Species = 122 }], }, // Mr. Mime @ Route 20
        new() { Location = 127, Slots = [new() { Species = 166 }], }, // Ledian @ Route 22
    ];

    private static readonly EncounterSlot5[] SlotsB2W2_HiddenGrottoEncounterSlots =
    [
        // reference http://bulbapedia.bulbagarden.net/wiki/Hidden_Grotto
        // Route 2
        new() { Species = 029, LevelMin = 55, LevelMax = 60, }, // Nidoran♀
        new() { Species = 032, LevelMin = 55, LevelMax = 60, }, // Nidoran♂
        new() { Species = 210, LevelMin = 55, LevelMax = 60, }, // Granbull
        new() { Species = 505, LevelMin = 55, LevelMax = 60, }, // Watchog

        // Route 3
        new() { Species = 310, LevelMin = 55, LevelMax = 60, }, // Manectric @ Dark Grass
        new() { Species = 417, LevelMin = 55, LevelMax = 60, }, // Pachirisu @ Dark Grass
        new() { Species = 523, LevelMin = 55, LevelMax = 60, }, // Zebstrika @ Dark Grass
        new() { Species = 048, LevelMin = 55, LevelMax = 60, }, // Venonat @ Pond
        new() { Species = 271, LevelMin = 55, LevelMax = 60, }, // Lombre @ Pond
        new() { Species = 400, LevelMin = 55, LevelMax = 60, }, // Bibarel @ Pond

        // Route 5
        new() { Species = 510, LevelMin = 20, LevelMax = 25, }, // Liepard
        new() { Species = 572, LevelMin = 20, LevelMax = 25, }, // Minccino
        new() { Species = 590, LevelMin = 20, LevelMax = 25, }, // Foongus

        // Route 6
        new() { Species = 206, LevelMin = 25, LevelMax = 30, }, // Dunsparce @ Near PKM Breeder
        new() { Species = 299, LevelMin = 25, LevelMax = 30, }, // Nosepass @ Mistralton Cave
        new() { Species = 527, LevelMin = 25, LevelMax = 30, }, // Woobat @ Both
        new() { Species = 590, LevelMin = 25, LevelMax = 30, }, // Foongus @ Both

        // Route 7
        new() { Species = 335, LevelMin = 30, LevelMax = 35, }, // Zangoose
        new() { Species = 336, LevelMin = 30, LevelMax = 35, }, // Seviper
        new() { Species = 505, LevelMin = 30, LevelMax = 35, }, // Watchog
        new() { Species = 613, LevelMin = 30, LevelMax = 35, }, // Cubchoo

        // Route 9
        new() { Species = 089, LevelMin = 35, LevelMax = 40, }, // Muk
        new() { Species = 510, LevelMin = 35, LevelMax = 40, }, // Liepard
        new() { Species = 569, LevelMin = 35, LevelMax = 40, }, // Garbodor
        new() { Species = 626, LevelMin = 35, LevelMax = 40, }, // Bouffalant

        // Route 13
        new() { Species = 114, LevelMin = 35, LevelMax = 40, }, // Tangela @ Gaint Chasm
        new() { Species = 363, LevelMin = 35, LevelMax = 40, }, // Spheal @ Stairs
        new() { Species = 425, LevelMin = 35, LevelMax = 40, }, // Drifloon @ Stairs
        new() { Species = 451, LevelMin = 35, LevelMax = 40, }, // Skorupi @ Gaint Chasm
        new() { Species = 590, LevelMin = 35, LevelMax = 40, }, // Foongus @ Both

        // Route 18
        new() { Species = 099, LevelMin = 55, LevelMax = 60, }, // Kingler
        new() { Species = 149, LevelMin = 55, LevelMax = 60, }, // Dragonite
        new() { Species = 222, LevelMin = 55, LevelMax = 60, }, // Corsola
        new() { Species = 441, LevelMin = 55, LevelMax = 60, }, // Chatot

        // Pinwheel Forest
        new() { Species = 061, LevelMin = 55, LevelMax = 60, }, // Poliwhirl @ Outer
        new() { Species = 198, LevelMin = 55, LevelMax = 60, }, // Murkrow @ Inner
        new() { Species = 286, LevelMin = 55, LevelMax = 60, }, // Breloom @ Inner
        new() { Species = 297, LevelMin = 55, LevelMax = 60, }, // Hariyama @ Outer
        new() { Species = 308, LevelMin = 55, LevelMax = 60, }, // Medicham @ Outer
        new() { Species = 371, LevelMin = 55, LevelMax = 60, }, // Bagon @ Outer
        new() { Species = 591, LevelMin = 55, LevelMax = 60, }, // Amoonguss @ Inner

        // Giant Chasm
        new() { Species = 035, LevelMin = 45, LevelMax = 50, }, // Clefairy
        new() { Species = 132, LevelMin = 45, LevelMax = 50, }, // Ditto
        new() { Species = 215, LevelMin = 45, LevelMax = 50, }, // Sneasel
        new() { Species = 375, LevelMin = 45, LevelMax = 50, }, // Metang

        // Abundant Shrine
        new() { Species = 037, LevelMin = 35, LevelMax = 40, }, // Vulpix @ Near Youngster
        new() { Species = 055, LevelMin = 35, LevelMax = 40, }, // Golduck @ Shrine
        new() { Species = 333, LevelMin = 35, LevelMax = 40, }, // Swablu @ Shrine
        new() { Species = 436, LevelMin = 35, LevelMax = 40, }, // Bronzor @ Near Youngster
        new() { Species = 591, LevelMin = 35, LevelMax = 40, }, // Amoonguss @ Both

        // Lostlorn Forest
        new() { Species = 127, LevelMin = 20, LevelMax = 25, }, // Pinsir
        new() { Species = 214, LevelMin = 20, LevelMax = 25, }, // Heracross
        new() { Species = 415, LevelMin = 20, LevelMax = 25, }, // Combee
        new() { Species = 542, LevelMin = 20, LevelMax = 25, }, // Leavanny

        // Route 22
        new() { Species = 279, LevelMin = 40, LevelMax = 45, }, // Pelipper
        new() { Species = 591, LevelMin = 40, LevelMax = 45, }, // Amoonguss
        new() { Species = 619, LevelMin = 40, LevelMax = 45, }, // Mienfoo

        // Route 23
        new() { Species = 055, LevelMin = 50, LevelMax = 55, }, // Golduck
        new() { Species = 207, LevelMin = 50, LevelMax = 55, }, // Gligar
        new() { Species = 335, LevelMin = 50, LevelMax = 55, }, // Zangoose
        new() { Species = 336, LevelMin = 50, LevelMax = 55, }, // Seviper
        new() { Species = 359, LevelMin = 50, LevelMax = 55, }, // Absol

        // Floccesy Ranch
        new() { Species = 183, LevelMin = 10, LevelMax = 15, }, // Marill
        new() { Species = 206, LevelMin = 10, LevelMax = 15, }, // Dunsparce
        new() { Species = 507, LevelMin = 10, LevelMax = 15, }, // Herdier

        // Funfest Missions
        // todo : check the level
        new() { Species = 133, LevelMin = 15, LevelMax = 60, }, // Eevee
        new() { Species = 134, LevelMin = 15, LevelMax = 60, }, // Vaporeon
        new() { Species = 135, LevelMin = 15, LevelMax = 60, }, // Jolteon
        new() { Species = 136, LevelMin = 15, LevelMax = 60, }, // Flareon
        new() { Species = 196, LevelMin = 15, LevelMax = 60, }, // Espeon
        new() { Species = 197, LevelMin = 15, LevelMax = 60, }, // Umbreon
        new() { Species = 431, LevelMin = 15, LevelMax = 60, }, // Glameow
        new() { Species = 434, LevelMin = 15, LevelMax = 60, }, // Stunky
        new() { Species = 470, LevelMin = 15, LevelMax = 60, }, // Leafeon
        new() { Species = 471, LevelMin = 15, LevelMax = 60, }, // Glaceon

        // Funfest Week 3
        // new() { Species = 060, LevelMin = 15, LevelMax = 60, }, // Poliwag
        new() { Species = 113, LevelMin = 15, LevelMax = 60, }, // Chansey
        new() { Species = 176, LevelMin = 15, LevelMax = 60, }, // Togetic
        new() { Species = 082, LevelMin = 15, LevelMax = 60, }, // Magneton
        new() { Species = 148, LevelMin = 15, LevelMax = 60, }, // Dragonair
        new() { Species = 372, LevelMin = 15, LevelMax = 60, } // Shelgon                      
    ];

    private static readonly EncounterArea5 SlotsB2_HiddenGrotto = new()
    {
        Location = 143, // Hidden Grotto
        Slots = [.. SlotsB2W2_HiddenGrottoEncounterSlots,
            new() { Species = 015, LevelMin = 55, LevelMax = 60 }, // Beedrill @ Pinwheel Forest
        ],
    };

    private static readonly EncounterArea5 SlotsW2_HiddenGrotto = new()
    {
        Location = 143, // Hidden Grotto
        Slots = [.. SlotsB2W2_HiddenGrottoEncounterSlots,
            new() { Species = 012, LevelMin = 55, LevelMax = 60 }, // Butterfree @ Pinwheel Forest
        ],
    };

    #endregion
}
