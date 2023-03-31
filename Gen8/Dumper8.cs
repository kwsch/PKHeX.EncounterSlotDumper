using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PKHeX.EncounterSlotDumper;

public static class Dumper8
{
    public static void DumpGen8()
    {
        DumpRegularRaids();
        DumpBCAT();
        DumpUnderground();
    }

    private static void DumpRegularRaids()
    {
        var sw = Nest8.Nest_Common.Concat(Nest8.Nest_SW)
            .OrderBy(z => z.Species).ThenBy(z => z.Form).ToArray();
        var sh = Nest8.Nest_Common.Concat(Nest8.Nest_SH)
            .OrderBy(z => z.Species).ThenBy(z => z.Form).ToArray();

        var bw = sw.SelectMany(z => z.Write()).ToArray();
        var bh = sh.SelectMany(z => z.Write()).ToArray();
        File.WriteAllBytes("encounter_sw_nest.pkl", bw);
        File.WriteAllBytes("encounter_sh_nest.pkl", bh);
    }

    private static void DumpBCAT()
    {
        var all = Encounters8Nest.Dist_Base.Concat(Encounters8Nest.Dist_DLC1.Concat(Encounters8Nest.Dist_DLC2)).ToArray();
        const byte verSW = 44;
        const byte verSH = 45;
        var bw = GetNiceBinary(all, verSW);
        var bh = GetNiceBinary(all, verSH);
        File.WriteAllBytes("encounter_sw_dist.pkl", bw);
        File.WriteAllBytes("encounter_sh_dist.pkl", bh);
    }

    private static void DumpUnderground()
    {
        var all = EStat8U.DynAdv_SWSH
            .OrderBy(z => z.Species)
            .ThenBy(z => z.Form)
            .SelectMany(z => z.Write())
            .ToArray();
        File.WriteAllBytes("encounter_swsh_underground.pkl", all);
    }

    private static byte[] GetNiceBinary(IEnumerable<EncounterStatic8ND> arr, byte ver)
    {
        var ordered = arr
            .Where(z => z.IsGroup(ver))
            .OrderBy(z => z.Species)
            .ThenBy(z => z.Form)
            .ThenByDescending(z => z.Index)
            .ToArray();

        var list = new List<byte[]>();
        foreach (var entry in ordered)
        {
            var data = entry.Write();
            if (list.Any(z => Match(z, data)))
                continue;
            list.Add(data);
        }

        static bool Match(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            for (int i = 0; i < a.Length - 1; i++) // ignore Index
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        return list.SelectMany(z => z).ToArray();
    }
}