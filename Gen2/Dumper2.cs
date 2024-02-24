using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;
using static PKHeX.EncounterSlotDumper.SlotType2;

namespace PKHeX.EncounterSlotDumper;

public static class Dumper2
{
    public static void DumpGen2()
    {
        Event2.DumpGen2();

        var g = Resources.encounter_gold;
        var s = Resources.encounter_silver;
        var c = Resources.encounter_crystal;
        var ga = EncounterArea2.GetArray2GrassWater(g);
        var sa = EncounterArea2.GetArray2GrassWater(s);
        var ca = EncounterArea2.GetArray2GrassWater(c);
        // Copy met locations from Crystal's met locations (already pre-formatted)
        for (int i = 0; i < ca.Length; i++)
            ga[i].Location = sa[i].Location = ca[i].Location;

        // GS has different swarm ordering. Just manually apply the correct met location IDs.
        ReadOnlySpan<byte> gs_swarm =
        [
            18, 18, 18,
            25, 25, 25,
            44, 44, 44,
            35, 35, 35,
            35
        ];
        for (int i = ga.Length - gs_swarm.Length, j = 0; i < ga.Length; i++, j++)
            ga[i].Location = sa[i].Location = gs_swarm[j];

        var gh = Resources.encounter_gold_h;
        var sh = Resources.encounter_silver_h;
        var ch = Resources.encounter_crystal_h;
        var gha = EncounterArea2.GetArray2Headbutt(gh);
        var sha = EncounterArea2.GetArray2Headbutt(sh);
        var cha = EncounterArea2.GetArray2Headbutt(ch);
        // Copy met locations from Crystal's met locations (already pre-formatted)
        for (int i = 0; i < cha.Length; i++)
            gha[i].Location = sha[i].Location = cha[i].Location;
        // Strip out the no-tree headbutt areas.
        {
            var gl = gha.ToList();
            var sl = sha.ToList();
            var cl = cha.ToList();

            static bool IsInaccessibleTree(EncounterArea2 z)
                => z.Type.IsHeadbutt() && !Dumper2h.Trees.ContainsKey(z.Location);

            gl.RemoveAll(IsInaccessibleTree);
            sl.RemoveAll(IsInaccessibleTree);
            cl.RemoveAll(IsInaccessibleTree);

            if (gha.Length == gl.Count || sha.Length == sl.Count || cha.Length == cl.Count)
                throw new Exception();
        }

        var fish = Resources.encounter_gsc_f;
        var f = EncounterArea2.GetArray2Fishing(fish);
        var gr = ga.Concat(gha).Concat(f)
            .Concat(new[] { EncounterBCC_GSC })
            .OrderBy(z => z.Location).ThenBy(z => z.Type);
        var sr = sa.Concat(sha).Concat(f)
            .Concat(new[] { EncounterBCC_GSC })
            .OrderBy(z => z.Location).ThenBy(z => z.Type);
        var cr = ca.Concat(cha).Concat(f)
            .Concat(new[] { EncounterBCC_GSC })
            .OrderBy(z => z.Location).ThenBy(z => z.Type);

        Write(gr, "encounter_gold.pkl");
        Write(sr, "encounter_silver.pkl");
        Write(cr, "encounter_crystal.pkl");
    }

    private static readonly EncounterArea2 EncounterBCC_GSC = new()
    {
        Location = 19,
        Type = BugContest,
        AreaRate = 20, // 40 in tall grass
        SlotRates = [20, 20, 10, 10, 05, 05, 10, 10, 05, 05],
        Slots =
        [
            new(010, 07, 18, 00), // Caterpie
            new(013, 07, 18, 01), // Weedle
            new(011, 09, 18, 02), // Metapod
            new(014, 09, 18, 03), // Kakuna
            new(012, 12, 15, 04), // Butterfree
            new(015, 12, 15, 05), // Beedrill
            new(048, 10, 16, 06), // Venonat
            new(046, 10, 17, 07), // Paras
            new(123, 13, 14, 08), // Scyther
            new(127, 13, 14, 09), // Pinsir
        ],
    };

    public static void Write(IEnumerable<EncounterArea2> areas, string name, string ident = "g2")
    {
        var serialized = areas.Select(Write).SelectMany(z => z);
        var unique = new List<byte[]>();
        int ctr = 0;
        foreach (var a in serialized)
        {
            ctr++;
            if (unique.Any(z => z.SequenceEqual(a)))
                continue;
            unique.Add(a);
        }

        var packed = BinLinker.Pack([.. unique], ident);
        File.WriteAllBytes(name, packed);
        Console.WriteLine($"Wrote {name} with {unique.Count} unique tables (originally {ctr}).");
    }

    public static IEnumerable<byte[]> Write(EncounterArea2 area)
    {
        if (area.SlotRates.Length == 0)
        {
            if (area.Type is not (Grass or Surf))
                throw new Exception("Invalid slot rate");
            yield return WriteTable(area);
        }
        else
        {
            if (area.Type is (Grass or Surf))
                throw new Exception("Invalid slot rate");
            yield return WriteTableVariableRates(area);
        }
    }

    private const byte SentinelVariableSlotRates = 0xFF;

    private static byte[] WriteTableVariableRates(EncounterArea2 area)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        var time = area.Slots.Max(z => z.Time);
        bw.Write((byte)area.Location);
        bw.Write((byte)time);
        bw.Write((byte)area.Type);
        bw.Write((byte)SentinelVariableSlotRates); // Area Rate sentinel to indicate multiple rates
        foreach (byte b in area.SlotRates)
            bw.Write(b);

        foreach (var s in area.Slots)
            WriteSlot(bw, s);
        return ms.ToArray();
    }

    private static byte[] WriteTable(EncounterArea2 area)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        var first = area.Slots[0];
        bw.Write((byte)area.Location);
        bw.Write((byte)first.Time);
        bw.Write((byte)area.Type);
        bw.Write((byte)area.AreaRate); // Area Rate
        if (area.AreaRate == SentinelVariableSlotRates)
            throw new Exception("Invalid slot rate");

        foreach (var s in area.Slots)
            WriteSlot(bw, s);
        return ms.ToArray();
    }

    private static void WriteSlot(BinaryWriter bw, EncounterSlot2 slot)
    {
        bw.Write((byte)slot.Species);
        bw.Write((byte)slot.SlotNumber);
        bw.Write((byte)slot.LevelMin);
        bw.Write((byte)slot.LevelMax);
    }
}

public static class Extensions
{
    public static bool IsHeadbutt(this SlotType2 t) => t is Headbutt or HeadbuttSpecial;
}
