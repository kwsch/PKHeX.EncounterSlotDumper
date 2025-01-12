using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper;

public static class Dumper7b
{
    private static byte[][] Get(byte[] data, string ident) => BinLinker.Unpack(data, ident);

    public static void DumpGen7b()
    {
        EncounterArea7b[] SlotsGP = EncounterArea7b.GetAreas(Get(Resources.encounter_gp, "gg"));
        EncounterArea7b[] SlotsGE = EncounterArea7b.GetAreas(Get(Resources.encounter_ge, "gg"));
        ManuallyAddRareSpawns(SlotsGP);
        ManuallyAddRareSpawns(SlotsGE);

        var gp = SlotsGP.Select(z => z.WriteSlots()).ToArray();
        var ge = SlotsGE.Select(z => z.WriteSlots()).ToArray();
        File.WriteAllBytes("encounter_gp.pkl", BinLinker.Pack(gp, "gg"));
        File.WriteAllBytes("encounter_ge.pkl", BinLinker.Pack(ge, "gg"));
    }

    private class RareSpawn
    {
        public readonly ushort Species;
        public readonly byte[] Locations;

        protected internal RareSpawn(ushort species, params byte[] locations)
        {
            Species = species;
            Locations = locations;
        }
    }

    private static readonly byte[] Sky =
    [
        003, 004, 005, 006, 009, 010, 011, 012, 013, 014, 015, 016, 017, 018, 019, 020, 021, 022, 023, 024, 025, 026, 027
    ];

#pragma warning disable IDE0230 // Use UTF-8 string literal
    private static readonly RareSpawn[] Rare =
    [
        // Normal
        new(001, 039),
        new(004, 005, 006, 041),
        new(007, 026, 027, 044),
        new(106, 045),
        new(107, 045),
        new(113, 007, 008, 010, 011, 012, 013, 014, 015, 016, 017, 018, 019, 020, 023, 025, 040, 042, 043, 045, 047, 051),
        new(137, 009),
        new(143, 046),

        // Water
        new(131, 021, 022),

        // Fly
        new(006, Sky),
        new(144, Sky),
        new(145, Sky),
        new(146, Sky),
        new(149, Sky)
    ];
#pragma warning restore IDE0230 // Use UTF-8 string literal

    private static void ManuallyAddRareSpawns(IEnumerable<EncounterArea7b> areas)
    {
        foreach (var table in areas)
        {
            var loc = table.Location;
            var species = Rare.Where(z => z.Locations.Contains((byte)loc))
                .Select(z => z.Species).ToArray();
            if (species.Length == 0)
                continue;

            var slots = table.Slots;
            var extra = species
                .Select(s => GetSlot(s, slots, loc, table.ToArea1, table.ToArea2)).ToArray();
            table.Slots = [..slots, ..extra];
        }
    }

    private static EncounterSlot7b GetSlot(ushort species, ReadOnlySpan<EncounterSlot7b> others, ushort loc, byte toarea1, byte toarea2)
    {
        var min = GetMinLevel(species, others, loc);
        var max = GetMaxLevel(species, others, loc);
        byte flags = EncounterArea7b.GetCrossoverAreaFlags(species, min, max, loc, toarea1, toarea2);
        return new EncounterSlot7b(species, min, max, flags);
    }

    private static byte GetMaxLevel(ushort species, ReadOnlySpan<EncounterSlot7b> slots, ushort loc)
    {
        if (loc == 22 && species == 131) // Route 20 Lapras
            return 44; // Slot tables were already merged. Just merge the resulting Lapras'es.
        return (species is 006 or >= 144) ? (byte)56 : slots[0].LevelMax;
    }

    private static byte GetMinLevel(ushort species, ReadOnlySpan<EncounterSlot7b> slots, ushort loc)
    {
        if (loc == 22 && species == 131) // Route 20 Lapras
            return 37; // Slot tables were already merged. Just merge the resulting Lapras'es.
        return (species is 006 or >= 144) ? (byte)03 : slots[0].LevelMin;
    }
}
