using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper;

public static class Dumper2h
{
    private static JsonSerializerOptions GetOpt() => new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static void Dump()
    {
        var raw = Resources.trees_h_c;
        var entries = BinLinker.Unpack(raw, "ch");
        var t = TreesArea.GetArray(entries);

        var lines = Resources.text_gsc_00000_en.Split('\n');
        var result = t.SelectMany(z => z.DumpLocation(lines));
        File.WriteAllLines("trees.txt", result);
        var json = Resources.trees;
        var listing = JsonSerializer.Deserialize<TreeAreaListing>(json, GetOpt());

        var tables = new List<TreeInfo>();
        foreach (var l in listing!.Table)
        {
            var info = new TreeInfo(l.Location);
            foreach (var tree in l.Valid)
                info.Add(tree);

            tables.Add(info);
        }

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        foreach (var info in tables)
            bw.Write(info.Write());

        var flat = ms.ToArray();
        File.WriteAllBytes("trees.bin", flat);
        File.WriteAllLines("tree_dict.txt", tables.Select(z => z.WriteString()));
    }

    public static readonly Dictionary<int, int> Trees = new()
    {
        { 02, 0x3FF_3FF }, // Route 29
        { 04, 0x0FF_3FF }, // Route 30
        { 05, 0x3FE_3FF }, // Route 31
        { 08, 0x3EE_3FF }, // Route 32
        { 11, 0x240_3FF }, // Route 33
        { 12, 0x37F_3FF }, // Azalea Town
        { 14, 0x3FF_3FF }, // Ilex Forest
        { 15, 0x001_3FE }, // Route 34
        { 18, 0x261_3FF }, // Route 35
        { 20, 0x3FF_3FF }, // Route 36
        { 21, 0x2B9_3FF }, // Route 37
        { 25, 0x3FF_3FF }, // Route 38
        { 26, 0x184_3FF }, // Route 39
        { 34, 0x3FF_3FF }, // Route 42
        { 37, 0x3FF_3FF }, // Route 43
        { 38, 0x3FF_3FF }, // Lake of Rage
        { 39, 0x2FF_3FF }, // Route 44
        { 91, 0x200_1FF }, // Route 26
        { 92, 0x2BB_3FF }, // Route 27
    };

    public class TreeInfo(in byte location)
    {
        public readonly byte Location = location;
        private ushort High;
        private ushort Moderate;

        public void Add(Tree t)
        {
            // Add 4 to each of the coordinates to convert to padded coordinates
            var x = t.X + 4;
            var y = t.Y + 4;
            var pivot = (((x * y) + x + y) / 5 % 10);

            High |= (ushort) (1 << pivot);
            for (int i = 0; i < 10; i++)
            {
                if (i == pivot)
                    continue;
                Moderate |= (ushort) (1 << i);
            }
        }

        public int Write() => (High << 8) | (Moderate << 20) | Location;
        public string WriteString() => $"{{{Location:00}, 0x{High:X3}_{Moderate:X3}}}";
    }
}
