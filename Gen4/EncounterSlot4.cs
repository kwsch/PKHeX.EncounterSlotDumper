namespace PKHeX.EncounterSlotDumper
{
    public sealed record EncounterSlot4 : EncounterSlot, IMagnetStatic, INumberedSlot
    {
        public int StaticIndex { get; set; }
        public int MagnetPullIndex { get; set; }
        public int StaticCount { get; set; }
        public int MagnetPullCount { get; set; }

        public int SlotNumber { get; set; }

        public int Level
        {
            set => LevelMin = LevelMax = value;
        }
    }
}