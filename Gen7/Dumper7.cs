using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static PKHeX.EncounterSlotDumper.Properties.Resources;
using static PKHeX.EncounterSlotDumper.SlotType7;

namespace PKHeX.EncounterSlotDumper;

public static class Dumper7
{
    public static void DumpGen7()
    {
        static EncounterArea7[] GetEncounterTables(string ident, byte[] mini)
        {
            var data = BinLinker.Unpack(mini, ident);
            return EncounterArea7.GetArray(data);
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

        byte[] pelagoMin = [1, 11, 21, 37, 49];
        InitializePelagoSM(pelagoMin, out var p_sn, out var p_mn);
        InitializePelagoUltra(pelagoMin, out var p_us, out var p_um);

        EncounterArea7[] SlotsSN = [..REG_SN, ..SOS_SN, ..p_sn];
        EncounterArea7[] SlotsMN = [..REG_MN, ..SOS_MN, ..p_mn];
        EncounterArea7[] SlotsUS = [..REG_US, ..SOS_US, ..p_us];
        EncounterArea7[] SlotsUM = [..REG_UM, ..SOS_UM, ..p_um];

        UpdateMiniorForm(SlotsSN, SlotsMN, SlotsUS, SlotsUM);

        Write(SlotsSN, "encounter_sn.pkl", "sm");
        Write(SlotsMN, "encounter_mn.pkl", "sm");

        Write(SlotsUS, "encounter_us.pkl", "uu");
        Write(SlotsUM, "encounter_um.pkl", "uu");
    }

    private static void UpdateMiniorForm(params EncounterArea7[][] gameTables)
    {
        foreach (var gameTable in gameTables)
        {
            foreach (var table in gameTable)
            {
                foreach (var slot in table.Slots)
                {
                    if (slot.Species == (int)Species.Minior)
                        slot.Form = 31; // "Random", template data sets 0, and game checks if species is minior. Let's be clean with our data.
                }
            }
        }
    }

    public static void Write(IEnumerable<EncounterArea7> area, string name, string ident = "g6")
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

    public static byte[] Write(EncounterArea7 area)
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

    private static void WriteSlot(BinaryWriter bw, EncounterSlot7 slot)
    {
        bw.Write((ushort)(slot.Species | (slot.Form << 11)));
        bw.Write((byte)slot.LevelMin);
        bw.Write((byte)slot.LevelMax);
    }

    private static void MarkAreasAsSOS(ref EncounterArea7[] Areas)
    {
        foreach (var area in Areas)
            area.Type = SOS;
    }

    private static void InitializePelagoSM(byte[] minLevels, out EncounterArea7[] sn, out EncounterArea7[] mn)
    {
        ushort[][] speciesSM =
        [
            [627/*SN*/, 021, 041, 090, 278, 731], // 1-7
            [064, 081, 092, 198, 426, 703],       // 11-17
            [060, 120, 127, 661, 709, 771],       // 21-27
            [227, 375, 707],                      // 37-43
            [123, 131, 429, 587] // 49-55
        ];
        sn = GetPelagoArea(speciesSM, minLevels);
        speciesSM[0][0] = 629; // Rufflet -> Vullaby
        mn = GetPelagoArea(speciesSM, minLevels);
    }

    private static void InitializePelagoUltra(byte[] minLevels, out EncounterArea7[] us, out EncounterArea7[] um)
    {
        ushort[][] speciesUU =
        [
            [731, 278, 041, 742, 086],        // 1-7
            [079, 120, 222, 122, 180, 124],   // 11-17
            [127, 177, 764, 163, 771, 701],   // 21-27
            [131, 354, 200, /* US  */ 228],   // 37-43
            [209, 667, 357, 430] // 49-55
        ];
        us = GetPelagoArea(speciesUU, minLevels);
        speciesUU[3][3] = 309; // Houndour -> Electrike
        um = GetPelagoArea(speciesUU, minLevels);
    }

    private static EncounterArea7[] GetPelagoArea(ushort[][] species, byte[] min)
    {
        // Species that appear at a lower level than the current table show up too.
        var area = new EncounterArea7
        {
            Type = Standard,
            Location = 30016,
            Slots = species.SelectMany((_, i) =>
                species.Take(1 + i).SelectMany(z => // grab current row & above
                    z.Select(s => new EncounterSlot7 // get slot data for each species
                        {
                            Species = s,
                            LevelMin = min[i],
                            LevelMax = (byte)(min[i] + 6)
                        }
                    ))).ToArray(),
        };
        return [area];
    }
}
