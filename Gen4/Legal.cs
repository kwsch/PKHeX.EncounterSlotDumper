using System;

namespace PKHeX.EncounterSlotDumper;

public static class Legal
{
    internal static ReadOnlySpan<byte> Slot4_Swarm => [0, 1];
    internal static ReadOnlySpan<byte> Slot4_Time => [2, 3];
    internal static ReadOnlySpan<byte> Slot4_Sound => [2, 3, 4, 5];
    internal static ReadOnlySpan<byte> Slot4_Radar => [4, 5, 10, 11];
    internal static ReadOnlySpan<byte> Slot4_Dual => [8, 9];
}
