using System;

namespace PKHeX.EncounterSlotDumper;

[Flags]
public enum EncounterType
{
    Undefined = 0,
    None = 1 << 00,
    RockSmash = 1 << 01,
    TallGrass = 1 << 02,
    DialgaPalkia = 1 << 04,
    Cave_HallOfOrigin = 1 << 05,
    Surfing_Fishing = 1 << 07,
    Building_EnigmaStone = 1 << 09,
    MarshSafari = 1 << 10,
    Starter_Fossil_Gift_DP = 1 << 12,
    DistortionWorld_Pt = 1 << 23,
    Starter_Fossil_Gift_Pt_DPTrio = 1 << 24,
}
