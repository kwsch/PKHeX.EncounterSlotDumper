namespace PKHeX.EncounterSlotDumper
{
    /// <summary>
    /// Wild Encounter Slot data
    /// </summary>
    public abstract class EncounterSlot
    {
        public int Species { get; set; }
        public int Form { get; set; }
        public int LevelMin { get; set; }
        public int LevelMax { get; set; }

        public EncounterSlot Clone() => (EncounterSlot)MemberwiseClone();

        public override string ToString() => $"{(Species) Species}{(Form == 0 ? "" : $"-{Form}")} @ {LevelMin}-{LevelMax}";
    }
}