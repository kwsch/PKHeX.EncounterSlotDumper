using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper;

public static class Dumper1
{
    public static void DumpGen1()
    {
        var r = Resources.encounter_red;
        var b = Resources.encounter_blue;
        var y = Resources.encounter_yellow;

        var rbf = Resources.encounter_rb_f;
        var yf = Resources.encounter_yellow_f;

        var red_gw = EncounterArea1.GetArray1GrassWater(r, 248);
        var blu_gw = EncounterArea1.GetArray1GrassWater(b, 248);
        var ylw_gw = EncounterArea1.GetArray1GrassWater(y, 249);
        var rb_fish = EncounterArea1.GetArray1Fishing(rbf, 33);
        var ylw_fish = EncounterArea1.GetArray1FishingYellow(yf);

        foreach (var area in red_gw)
            area.Location = RBYLocIndexes[area.Location];
        foreach (var area in blu_gw)
            area.Location = RBYLocIndexes[area.Location];
        foreach (var area in ylw_gw)
            area.Location = RBYLocIndexes[area.Location];

        for (var i = 0; i < rb_fish.Length; i++)
            rb_fish[i].Location = RBFishIndexes[i];
        rb_fish = rb_fish.Where(z => z.Location != 0).ToArray(); // remove duplicate locations (cerulean gym same as cerulean city)
        for (var i = 0; i < ylw_fish.Length; i++)
            ylw_fish[i].Location = YFishIndexes[i];

        rb_fish  = [EncounterArea1.FishOld_RBY, EncounterArea1.FishGood_RBY, .. rb_fish];
        ylw_fish = [EncounterArea1.FishOld_RBY, EncounterArea1.FishGood_RBY, .. ylw_fish];
        var rb = red_gw.Concat(rb_fish).OrderBy(z => z.Location).ThenBy(z => z.Type).ToArray();
        var bb = blu_gw.Concat(rb_fish).OrderBy(z => z.Location).ThenBy(z => z.Type).ToArray();
        var yb = ylw_gw.Concat(ylw_fish).OrderBy(z => z.Location).ThenBy(z => z.Type).ToArray();

        Write(rb, "encounter_red.pkl");
        Write(bb, "encounter_blue.pkl");
        Write(yb, "encounter_yellow.pkl");

        var jp_bu = DumpGen1JPBU();
        Write(rb.Concat(bb).Concat(yb), jp_bu, "encounter_blue_jp.pkl");
    }

    public static IOrderedEnumerable<EncounterArea1> DumpGen1JPBU()
    {
        var b = Resources.encounter_blue_jp;

        var rbf = Resources.encounter_blue_jp_f;

        var blu_gw = EncounterArea1.GetArray1GrassWater(b, 248);
        var rb_fish = EncounterArea1.GetArray1Fishing(rbf, 33);

        foreach (var area in blu_gw)
            area.Location = RBYLocIndexes[area.Location];

        for (var i = 0; i < rb_fish.Length; i++)
            rb_fish[i].Location = RBFishIndexes[i];
        rb_fish = rb_fish.Where(z => z.Location != 0).ToArray(); // remove duplicate locations (cerulean gym same as cerulean city)

        return blu_gw.Concat(rb_fish).OrderBy(z => z.Location).ThenBy(z => z.Type);
    }

    public static void Write(IEnumerable<EncounterArea1> area,
        [ConstantExpected] string name, [ConstantExpected] string ident = "g1")
    {
        var serialized = area.Select(Write).ToArray();
        List<byte[]> unique = [];
        foreach (var a in serialized)
        {
            if (unique.Any(z => z.SequenceEqual(a)))
                continue;
            unique.Add(a);
        }

        PackUnique(unique, name, ident);
        Console.WriteLine($"Wrote {name} with {unique.Count} unique tables (originally {serialized.Length}).");
    }

    public static void Write(IEnumerable<EncounterArea1> other, IEnumerable<EncounterArea1> revised,
        [ConstantExpected] string name, [ConstantExpected] string ident = "g1")
    {
        var serializedOther = other.Select(Write).ToArray();
        var serializedRevised = revised.Select(Write).ToArray();
        List<byte[]> unique = [];
        foreach (var a in serializedOther)
        {
            if (unique.Any(z => z.SequenceEqual(a)))
                continue;
            unique.Add(a);
        }

        List<byte[]> clean = [];
        foreach (var a in serializedRevised)
        {
            if (unique.Any(z => z.SequenceEqual(a)))
                continue;
            unique.Add(a);
            clean.Add(a);
        }

        PackUnique(clean, name, ident);
        Console.WriteLine($"Wrote {name} with {clean.Count} unique tables (originally {serializedRevised.Length}).");
    }

    private static void PackUnique(List<byte[]> unique,
        [ConstantExpected] string name, [ConstantExpected] string ident)
    {
        var packed = BinLinker.Pack([.. unique], ident);
        File.WriteAllBytes(name, packed);
    }

    public static byte[] Write(EncounterArea1 area)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write((ushort)area.Location);
        bw.Write((byte)area.Type);
        bw.Write((byte)area.Rate);

        foreach (var slot in area.Slots)
            WriteSlot(bw, slot);

        return ms.ToArray();
    }

    private static void WriteSlot(BinaryWriter bw, EncounterSlot1 slot)
    {
        bw.Write((byte)slot.Species);
        bw.Write((byte)slot.SlotNumber);
        bw.Write((byte)slot.LevelMin);
        bw.Write((byte)slot.LevelMax);
    }

    // FR/LG location indexes, since they are a superset of RBY.
    private static ReadOnlySpan<byte> RBYLocIndexes =>
    [
        088, 089, 090, 091, 092, 093, 094, 095, 096, 097,
        098, 000, 101, 102, 103, 104, 105, 106, 107, 108,
        109, 110, 111, 112, 113, 114, 115, 116, 117, 118,
        119, 120, 121, 122, 123, 124, 125, 000, 000, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 000, 000,
        000, 126, 000, 000, 000, 000, 000, 000, 000, 127,
        127, 127, 000, 000, 000, 000, 000, 000, 000, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 000, 000,
        000, 000, 138, 142, 000, 000, 000, 000, 000, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 000, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 132, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 000, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 000, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 000, 000,
        000, 000, 140, 140, 140, 140, 140, 140, 140, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 000, 139,
        139, 139, 139, 000, 000, 135, 000, 000, 000, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 000, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 000, 000,
        000, 000, 139, 000, 132, 000, 000, 131, 132, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 000, 000,
        000, 000, 000, 000, 135, 135, 135, 136, 136, 136,
        136, 000, 000, 000, 000, 000, 141, 141, 141, 000,
        000, 000, 138, 000, 000, 000, 000, 000, 000, 000,
        000, 000, 000, 000, 000, 000, 000, 000, 000, 000,
    ];

    private static ReadOnlySpan<byte> RBFishIndexes =>
    [
        // Dummy out duplicate groups with same met location
        088, 089, 091, 093, 094, 095, 096, 104, 106, 110,
        111, 112, 113, 117, 118, 119, 120, 121, 122, 123,
        124, 125,   0,   0, 139,   0, 136,   0,   0,   0,
        141,   0,   0
    ];

    private static ReadOnlySpan<byte> YFishIndexes =>
    [
        088, 089, 091, 093, 094, 095, 096, 104, 106, 124,
        125, 110, 111, 112, 113, 117, 118, 119, 120, 121,
        122, 123, 093, 136, 136, 136, 136, 139, 139, 141,
        141
    ];
}
