using System;

namespace PKHeX.EncounterSlotDumper;

public static class Encounters4
{
    public static ReadOnlySpan<ushort> TrophyDP => [ 035, 039, 052, 113, 133, 137, 173, 174, 183, 298, 311, 312, 351, 438, 439, 440 ]; // Porygon
    public static ReadOnlySpan<ushort> TrophyPt => [ 035, 039, 052, 113, 133, 132, 173, 174, 183, 298, 311, 312, 351, 438, 439, 440 ]; // Ditto

    // For marsh slots, remove duplicate (more frequent) species in favor of a single entry per species.
    public static ReadOnlySpan<ushort> MarshDP =>
    [
        // Daily changing Pokemon are not in the raw data http://bulbapedia.bulbagarden.net/wiki/Great_Marsh
        055, 315, 397, 451, 453, 455,
        183, 194, 195, 298, 399, 400,          // Pre-National Pokédex
        046, 102, 115, 193, 285, 316, 452, 454 // Post-National Pokédex
    ];

    public static ReadOnlySpan<ushort> MarshPt =>
    [
        114,193,195,357,451,453,455,194, // Pre-National Pokédex
        046,102,115,285,316,352,452,454, // Post-National Pokédex
    ];
}
