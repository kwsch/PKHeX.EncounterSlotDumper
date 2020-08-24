using System;
using System.Collections.Generic;
using System.Linq;

namespace PKHeX.EncounterSlotDumper
{
    public abstract class EncounterArea4 : EncounterArea
    {
        public int Rate { get; set; }
        public EncounterType TypeEncounter { get; set; } = EncounterType.None;

        /// <summary>
        /// Reads the GBA Pak Special slots, cloning <see cref="EncounterSlot"/> data from the area's base encounter slots.
        /// </summary>
        /// <remarks>
        /// These special slots only contain the info of species id; the level is copied from the corresponding <see cref="slotnums"/> index.
        /// </remarks>
        /// <param name="data">Encounter binary data</param>
        /// <param name="ofs">Offset to read from</param>
        /// <param name="slotSize">DP/Pt slotSize = 4 bytes/entry, HG/SS slotSize = 2 bytes/entry</param>
        /// <param name="ReplacedSlots">Slots from regular encounter table that end up replaced by in-game conditions</param>
        /// <param name="slotnums">Slot indexes to replace with read species IDs</param>
        protected static List<EncounterSlot4> GetSlots4GrassSlotReplace(byte[] data, int ofs, int slotSize, EncounterSlot[] ReplacedSlots, int[] slotnums)
        {
            var slots = new List<EncounterSlot4>();

            int numslots = slotnums.Length;
            for (int i = 0; i < numslots; i++)
            {
                var baseSlot = ReplacedSlots[slotnums[i]];
                if (baseSlot.LevelMin <= 0)
                    continue;

                int species = BitConverter.ToUInt16(data, ofs + (i / (4 / slotSize) * slotSize));
                if (species <= 0 || baseSlot.Species == species) // Empty or duplicate
                    continue;

                var slot = (EncounterSlot4)baseSlot.Clone();
                slot.Species = species;
                slot.SlotNumber = i;
                slots.Add(slot);
            }
            return slots;
        }

        protected static IEnumerable<EncounterSlot4> MarkStaticMagnetExtras(IEnumerable<IEnumerable<List<EncounterSlot4>>> product)
        {
            var trackPermute = new List<EncounterSlot4>();
            foreach (var p in product)
                MarkStaticMagnetPermute(p.SelectMany(z => z), trackPermute);
            return trackPermute;
        }

        protected static void MarkStaticMagnetPermute(IEnumerable<EncounterSlot4> grp, List<EncounterSlot4> trackPermute)
        {
            EncounterUtil.MarkEncountersStaticMagnetPullPermutation(grp, PersonalTable.HGSS, trackPermute);
        }
    }
}