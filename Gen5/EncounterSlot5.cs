namespace PKHeX.EncounterSlotDumper
{
    public sealed record EncounterSlot5 : EncounterSlot, INumberedSlot
    {
        public int SlotNumber { get; set; }
    }
}