
using System;

namespace PKHeX.EncounterSlotDumper
{
    public class EncounterArea
    {
        public short Location;
        public SlotType Type { get; set; } = SlotType.Any;
        public EncounterSlot[] Slots;

        public override string ToString() => $"{Type} @ {Location}: {Slots.Length} slots.";

        /// <summary>
        /// Gets the encounter areas for species with same level range and same slot type at same location
        /// </summary>
        /// <param name="species">List of species that exist in the Area.</param>
        /// <param name="lvls">Paired min and max levels of the encounter slots.</param>
        /// <param name="location">Location index of the encounter area.</param>
        /// <param name="t">Encounter slot type of the encounter area.</param>
        /// <returns>Encounter area with slots</returns>
        public static TArea[] GetSimpleEncounterArea<TArea, TSlot>(int[] species, int[] lvls, short location, SlotType t)
            where TArea : EncounterArea, new()
            where TSlot : EncounterSlot, INumberedSlot, new()
        {
            if ((lvls.Length & 1) != 0) // levels data not paired; expect multiple of 2
                throw new ArgumentException(nameof(lvls));

            var count = species.Length * (lvls.Length / 2);
            var slots = new TSlot[count];
            int ctr = 0;
            foreach (var s in species)
            {
                for (int i = 0; i < lvls.Length;)
                {
                    slots[ctr] = new TSlot
                    {
                        LevelMin = lvls[i++],
                        LevelMax = lvls[i++],
                        Species = s,
                        SlotNumber = ctr,
                    };
                    ctr++;
                }
            }
            return new[] { new TArea { Location = location, Slots = slots, Type = t } };
        }
    }
}