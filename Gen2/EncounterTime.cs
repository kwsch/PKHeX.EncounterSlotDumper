using System;

namespace PKHeX.EncounterSlotDumper;

[Flags]
internal enum EncounterTime
{
    Any = 0,
    Morning = 1 << 1,
    Day = 1 << 2,
    Night = 1 << 3,
}
