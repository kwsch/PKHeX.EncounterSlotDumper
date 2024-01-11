namespace PKHeX.EncounterSlotDumper
{
    /// <summary>
    /// Generation 1 Wild Encounter Slot data
    /// </summary>
    public sealed record EncounterSlot1 : EncounterSlot
    {
        public int SlotNumber { get; set; }

        public EncounterSlot1(int species, int min, int max, int slot)
        {
            Species = species;
            LevelMin = min;
            LevelMax = max;
            SlotNumber = slot;
        }

        /// <summary>
        /// Deserializes Gen1 Encounter Slots from data.
        /// </summary>
        /// <param name="data">Byte array containing complete slot data table.</param>
        /// <param name="ofs">Offset to start reading from.</param>
        /// <param name="count">Amount of slots to read.</param>
        /// <param name="type">Type of encounter slot table.</param>
        /// <returns>Array of encounter slots.</returns>
        public static EncounterSlot1[] ReadSlots(byte[] data, ref int ofs, int count, SlotType type)
        {
            var bump = type == SlotType.Surf ? 4 : 0;
            var slots = new EncounterSlot1[count];
            for (int slot = 0; slot < count; slot++)
            {
                int min = data[ofs++];
                int species = data[ofs++];
                int max = min + bump;
                slots[slot] = new EncounterSlot1(species, min, max, slot);
            }

            return slots;
        }

        public static readonly EncounterArea1 FishOld_RBY = new EncounterArea1
        {
            Location = -1, // Any
            Type = SlotType.Old_Rod,
            Rate = 100,
            Slots = new EncounterSlot[]
            {
                new EncounterSlot1(129, 05, 05, 0), // Magikarp
            }
        };

        public static readonly EncounterArea1 FishGood_RBY = new EncounterArea1
        {
            Location = -1, // Any
            Type = SlotType.Good_Rod,
            Rate = 100,
            Slots = new EncounterSlot[]
            {
                new EncounterSlot1(118, 10, 10, 0), // Goldeen
                new EncounterSlot1(060, 10, 10, 1), // Poliwag
            }
        };
    }
}