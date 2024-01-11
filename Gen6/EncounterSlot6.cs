namespace PKHeX.EncounterSlotDumper
{
    public sealed record EncounterSlot6 : EncounterSlot, INumberedSlot
    {
        public int SlotNumber { get; set; }
    }
}