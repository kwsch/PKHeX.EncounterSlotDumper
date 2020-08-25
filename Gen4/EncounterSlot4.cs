namespace PKHeX.EncounterSlotDumper
{
    public sealed class EncounterSlot4 : EncounterSlot, IMagnetStatic, INumberedSlot
    {
        public int StaticIndex { get; set; }
        public int MagnetPullIndex { get; set; }
        public int StaticCount { get; set; }
        public int MagnetPullCount { get; set; }

        public int SlotNumber { get; set; }
    }
}