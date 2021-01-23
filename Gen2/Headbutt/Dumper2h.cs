using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper
{
    public static class Dumper2h
    {
        public static void Dump()
        {
            var raw = Resources.trees_h_c;
            var entries = BinLinker.Unpack(raw, "ch");
            var t = TreesArea.GetArray(entries);

            var lines = Resources.text_gsc_00000_en.Split('\n');
            var result = t.SelectMany(z => z.DumpLocation(lines));
            File.WriteAllLines("trees.txt", result);

            var opt = new JsonSerializerOptions {ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true};
            var json = Resources.trees;
            var listing = JsonSerializer.Deserialize<TreeAreaListing>(json, opt);

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

        public class TreeInfo
        {
            public int Location;
            private ushort High;
            private ushort Moderate;

            public TreeInfo(in int location) => Location = location;

            public void Add(Tree t)
            {
                var x = t.X;
                var y = t.Y;
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
}
