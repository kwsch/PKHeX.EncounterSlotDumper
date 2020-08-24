using System.Collections.Generic;
using System.Linq;

namespace PKHeX.EncounterSlotDumper
{
    public static class EncounterUtil
    {
        /// <summary>
        /// Marks Encounter Slots for party lead's ability slot influencing.
        /// </summary>
        /// <remarks>Magnet Pull attracts Steel type slots, and Static attracts Electric</remarks>
        /// <param name="areas">Encounter Area array for game</param>
        /// <param name="t">Personal data for use with a given species' type</param>
        public static void MarkEncountersStaticMagnetPull<T>(IEnumerable<EncounterArea> areas, PersonalTable t)
            where T : EncounterSlot, IMagnetStatic
        {
            foreach (EncounterArea area in areas)
            {
                MarkEncountersStaticMagnetPull(area.Slots.Cast<T>(), t);
            }
        }

        internal static void MarkEncountersStaticMagnetPullPermutation<T>(IEnumerable<T> grp, PersonalTable t, List<T> permuted)
            where T : EncounterSlot, IMagnetStatic, INumberedSlot
        {
            GetStaticMagnet(t, grp, out List<T> s, out List<T> m);

            // Apply static/magnet values; if any permutation has a unique slot combination, add it to the slot list.
            for (int i = 0; i < s.Count; i++)
            {
                var slot = s[i];
                if (slot.StaticIndex >= 0) // already has unique data
                {
                    if (slot.IsMatchStatic(i, s.Count))
                        continue; // same values, no permutation
                    if (permuted.Any(z => z.SlotNumber == slot.SlotNumber && z.IsMatchStatic(i, s.Count) && z.Species == slot.Species))
                        continue; // same values, previously permuted

                    s[i] = slot = (T)slot.Clone();
                    permuted.Add(slot);
                }
                slot.StaticIndex = i;
                slot.StaticCount = s.Count;
            }
            for (int i = 0; i < m.Count; i++)
            {
                var slot = m[i];
                if (slot.MagnetPullIndex >= 0) // already has unique data
                {
                    if (slot.IsMatchStatic(i, m.Count))
                        continue; // same values, no permutation
                    if (permuted.Any(z => z.SlotNumber == slot.SlotNumber && z.IsMatchMagnet(i, m.Count) && z.Species == slot.Species))
                        continue; // same values, previously permuted

                    m[i] = slot = (T)slot.Clone();
                    permuted.Add(slot);
                }
                slot.MagnetPullIndex = i;
                slot.MagnetPullCount = m.Count;
            }
        }

        private static void MarkEncountersStaticMagnetPull<T>(IEnumerable<T> grp, PersonalTable t)
            where T : EncounterSlot, IMagnetStatic
        {
            GetStaticMagnet(t, grp, out List<T> s, out List<T> m);
            for (var i = 0; i < s.Count; i++)
            {
                var slot = s[i];
                slot.StaticIndex = i;
                slot.StaticCount = s.Count;
            }
            for (var i = 0; i < m.Count; i++)
            {
                var slot = m[i];
                slot.MagnetPullIndex = i;
                slot.MagnetPullCount = m.Count;
            }
        }

        private static void GetStaticMagnet<T>(PersonalTable t, IEnumerable<T> grp, out List<T> s, out List<T> m)
            where T : EncounterSlot, IMagnetStatic
        {
            const int steel = (int)MoveType.Steel;
            const int electric = (int)MoveType.Electric + 1; // offset by 1 in gen3/4 for the ??? type
            s = new List<T>();
            m = new List<T>();
            foreach (T Slot in grp)
            {
                var p = t[Slot.Species];
                if (p.IsType(steel))
                    m.Add(Slot);
                if (p.IsType(electric))
                    s.Add(Slot);
            }
        }
    }
}