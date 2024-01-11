namespace PKHeX.EncounterSlotDumper
{
    public sealed record EncounterSlot7 : EncounterSlot, INumberedSlot
    {
        public int SlotNumber { get; set; }
    }
}