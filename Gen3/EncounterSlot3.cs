using System.Collections.Generic;

namespace PKHeX.EncounterSlotDumper
{
    public record EncounterSlot3 : EncounterSlot, IMagnetStatic, INumberedSlot
    {
        public int StaticIndex { get; set; } = -1;
        public int MagnetPullIndex { get; set; } = -1;
        public int StaticCount { get; set; }
        public int MagnetPullCount { get; set; }

        public int SlotNumber { get; set; }
    }

    internal sealed record EncounterSlot3Swarm : EncounterSlot3
    {
        public IReadOnlyList<int> Moves { get; }

        public EncounterSlot3Swarm(IReadOnlyList<int> moves) => Moves = moves;
    }
}