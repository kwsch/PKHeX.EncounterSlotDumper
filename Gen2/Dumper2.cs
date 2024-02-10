using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;
using static PKHeX.EncounterSlotDumper.SlotType2;

namespace PKHeX.EncounterSlotDumper;

public static class Dumper2
{
    public static ReadOnlySpan<byte> BCC_SlotRates => [ 20, 20, 10, 10, 05, 05, 10, 10, 05, 05 ];

    public static void DumpGen2()
    {
        var g = Resources.encounter_gold;
        var s = Resources.encounter_silver;
        var c = Resources.encounter_crystal;

        var ga = EncounterArea2.GetArray2GrassWater(g);
        var sa = EncounterArea2.GetArray2GrassWater(s);
        var ca = EncounterArea2.GetArray2GrassWater(c);

        var gh = Resources.encounter_gold_h;
        var sh = Resources.encounter_silver_h;
        var ch = Resources.encounter_crystal_h;
        var fish = Resources.encounter_gsc_f;

        var gha = EncounterArea2.GetArray2Headbutt(gh);
        var sha = EncounterArea2.GetArray2Headbutt(sh);
        var cha = EncounterArea2.GetArray2Headbutt(ch);
        var f = EncounterArea2.GetArray2Fishing(fish);

        // Copy met locations from Crystal's met locations (already pre-formatted)
        for (int i = 0; i < ca.Length; i++)
            ga[i].Location = sa[i].Location = ca[i].Location;
        for (int i = 0; i < cha.Length; i++)
            gha[i].Location = sha[i].Location = cha[i].Location;

        // GS has different swarm ordering. Just manually apply the correct met location IDs.
        ReadOnlySpan<byte> gs_swarm = [ 18, 25, 44, 35, 35 ];
        for (int i = ga.Length - gs_swarm.Length, j = 0; i < ga.Length; i++, j++)
            ga[i].Location = sa[i].Location = gs_swarm[j];

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

        var gr = ga.Concat(gha).Concat(f)
            .Concat(new[] {EncounterBCC_GSC })
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
        Rates = [20], // 40 in tall grass
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
        var serialized = areas.Select(Write).SelectMany(z => z).ToArray();
        var unique = new List<byte[]>();
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

    public static IEnumerable<byte[]> Write(EncounterArea2 area)
    {
        var type = (area.Type) & (SlotType2)0xF;
        if (type == Grass)
        {
            var slotCount = area.Slots.Length / area.Rates.Length;
            for (var r = 0; r < area.Rates.Length; r++)
            {
                var rate = area.Rates[r];

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);

                bw.Write((byte)area.Location);
                int firstSlot = r * slotCount;
                var first = area.Slots[firstSlot];
                bw.Write((byte)first.Time);

                bw.Write((byte)area.Type);
                bw.Write(rate);

                for (int i = r * slotCount; i < (r + 1) * slotCount; i++)
                {
                    var slot = area.Slots[i];
                    WriteSlot(bw, slot);
                }
                yield return ms.ToArray();
            }
        }
        else if (area.Type is Old_Rod or Good_Rod or Super_Rod)
        {
            if (area.Slots.Length == area.Rates.Length)
            {
                yield return WriteTable(area);
                yield break;
            }

            var types = area.Slots.Select(z => z.Time).Distinct().ToList();
            types.RemoveAll(z => z == EncounterTime.Any);

            foreach (var t in types)
                yield return WriteTableOfTime(area, t);
        }
        else
        {
            yield return WriteTable(area);
        }
    }

    private static byte[] WriteTableOfTime(EncounterArea2 area, EncounterTime t)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        var slots = area.Slots
            .Where(z => z.Time == EncounterTime.Any || z.Time == t).ToArray();

        bw.Write((byte) area.Location);
        bw.Write((byte) t);

        bw.Write((byte) area.Type);
        bw.Write((byte) 0xFF);

        foreach (byte b in area.Rates)
            bw.Write(b);
        foreach (var s in slots)
            WriteSlot(bw, s);
        return ms.ToArray();
    }

    private static byte[] WriteTable(EncounterArea2 area)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write((byte) area.Location);
        var first = area.Slots[0];
        bw.Write((byte) first.Time);

        var type = (byte) area.Type;
        bw.Write(type);
        if ((SlotType2)(type & 0xF) == Surf)
        {
            bw.Write(area.Rates[0]);
        }
        else if ((SlotType2)type == BugContest)
        {
            bw.Write(area.Rates[0]);
        }
        else
        {
            bw.Write((byte)0xFF);
            if (area.Rates.Length == 1)
                throw new Exception();
            foreach (byte b in area.Rates)
                bw.Write(b);
        }

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
