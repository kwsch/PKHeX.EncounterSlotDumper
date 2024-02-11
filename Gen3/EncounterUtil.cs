using System.Collections.Generic;
using System.Linq;

namespace PKHeX.EncounterSlotDumper;

public abstract record EncounterSlot34
{
    public abstract ushort Species { get; init; }
}

public static class EncounterUtil
{
    /// <summary>
    /// Marks Encounter Slots for party lead's ability slot influencing.
    /// </summary>
    /// <remarks>Magnet Pull attracts Steel type slots, and Static attracts Electric</remarks>
    /// <param name="areas">Encounter Area array for game</param>
    /// <param name="t">Personal data for use with a given species' type</param>
    public static void MarkEncountersStaticMagnetPull(IEnumerable<EncounterArea3> areas, PersonalTable t)
    {
        foreach (var area in areas)
        {
            MarkEncountersStaticMagnetPull(area.Slots, t);
        }
    }

    public static void MarkEncountersStaticMagnetPull(EncounterArea4 area, PersonalTable t)
    {
        MarkEncountersStaticMagnetPull(area.Slots, t);
    }

    internal static void MarkEncountersStaticMagnetPullPermutation<T>(IEnumerable<T> grp, PersonalTable t, List<T> permuted)
        where T : EncounterSlot34, IMagnetStatic, INumberedSlot
    {
        GetStaticMagnet(t, grp, out var s, out var m);

        // Apply static/magnet values; if any permutation has a unique slot combination, add it to the slot list.
        for (int i = 0; i < s.Count; i++)
        {
            var slot = s[i];
            if (slot.StaticCount > 0) // already has unique data
            {
                if (slot.IsMatchStatic(i, s.Count))
                    continue; // same values, no permutation
                if (permuted.Any(z => z.SlotNumber == slot.SlotNumber && z.IsMatchStatic(i, s.Count) && z.Species == slot.Species))
                    continue; // same values, previously permuted

                s[i] = slot = slot with { };
                permuted.Add(slot);
            }
            slot.StaticIndex = (byte)i;
            slot.StaticCount = (byte)s.Count;
        }
        for (int i = 0; i < m.Count; i++)
        {
            var slot = m[i];
            if (slot.MagnetPullCount > 0) // already has unique data
            {
                if (slot.IsMatchStatic(i, m.Count))
                    continue; // same values, no permutation
                if (permuted.Any(z => z.SlotNumber == slot.SlotNumber && z.IsMatchMagnet(i, m.Count) && z.Species == slot.Species))
                    continue; // same values, previously permuted

                m[i] = slot = slot with { };
                permuted.Add(slot);
            }
            slot.MagnetPullIndex = (byte)i;
            slot.MagnetPullCount = (byte)m.Count;
        }
    }

    private static void MarkEncountersStaticMagnetPull<T>(IEnumerable<T> grp, PersonalTable t)
        where T : EncounterSlot34, IMagnetStatic
    {
        GetStaticMagnet(t, grp, out var s, out var m);
        for (var i = 0; i < s.Count; i++)
        {
            var slot = s[i];
            slot.StaticIndex = (byte)i;
            slot.StaticCount = (byte)s.Count;
        }
        for (var i = 0; i < m.Count; i++)
        {
            var slot = m[i];
            slot.MagnetPullIndex = (byte)i;
            slot.MagnetPullCount = (byte)m.Count;
        }
    }

    private static void GetStaticMagnet<T>(PersonalTable t, IEnumerable<T> grp, out List<T> s, out List<T> m)
        where T : EncounterSlot34, IMagnetStatic
    {
        const int steel = (int)MoveType.Steel;
        const int electric = (int)MoveType.Electric + 1; // offset by 1 in gen3/4 for the ??? type
        s = [];
        m = [];
        foreach (var slot in grp)
        {
            var p = t[slot.Species];
            if (p.IsType(steel))
                m.Add(slot);
            if (p.IsType(electric))
                s.Add(slot);
        }
    }
}
