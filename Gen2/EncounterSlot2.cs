namespace PKHeX.EncounterSlotDumper
{
    /// <summary>
    /// Generation 2 Wild Encounter Slot data
    /// </summary>
    /// <remarks>
    /// Contains Time data which is present in Crystal origin data.
    /// </remarks>
    public sealed class EncounterSlot2 : EncounterSlot
    {
        public int SlotNumber { get; set; }

        internal EncounterTime Time;

        public EncounterSlot2(int species, int min, int max, int slot)
        {
            Species = species;
            LevelMin = min;
            LevelMax = max;
            SlotNumber = slot;
        }

        public override string ToString() => $"{Time} - {base.ToString()}";

        /// <summary>
        /// Deserializes Gen2 Encounter Slots from data.
        /// </summary>
        /// <param name="data">Byte array containing complete slot data table.</param>
        /// <param name="ofs">Offset to start reading from.</param>
        /// <param name="count">Amount of slots to read.</param>
        /// <param name="type">Type of encounter slot table.</param>
        /// <returns>Array of encounter slots.</returns>
        public static EncounterSlot2[] ReadSlots(byte[] data, ref int ofs, int count, SlotType type)
        {
            var bump = type == SlotType.Surf ? 4 : 0;
            var slots = new EncounterSlot2[count];
            for (int slot = 0; slot < count; slot++)
            {
                int min = data[ofs++];
                int species = data[ofs++];
                int max = min + bump;
                slots[slot] = new EncounterSlot2(species, min, max, slot);
            }
            return slots;
        }
    }
}