using System;
using static PKHeX.EncounterSlotDumper.SlotType4;

namespace PKHeX.EncounterSlotDumper;

public sealed class SafariZone
{
    public readonly SafariHeader Header;
    public readonly SafariSlotSet Super;
    public readonly SafariSlotSet Good;
    public readonly SafariSlotSet Old;
    public readonly SafariSlotSet Surf;
    public readonly SafariSlotSet Grass;

    public SafariZone(ReadOnlySpan<byte> data)
    {
        Header = new(data);
        data = data[8..];

        const int sizeGrass = 0x118; // 3 sets of 10 slots, 10 extra slots, 10 blocksets
        const int sizeSurf = 0xA8; // 3 sets of 10 slots, 3 extra slots, 3 blocksets
        const int sizeFish = 0x98; // 3 sets of 10 slots, 2 extra slots, 2 blocksets
        var grass = data[..sizeGrass];
        var surf = data.Slice(sizeGrass, sizeSurf);
        var old = data.Slice(sizeGrass + sizeSurf, sizeFish);
        var good = data.Slice(sizeGrass + sizeSurf + sizeFish, sizeFish);
        var super = data.Slice(sizeGrass + sizeSurf + sizeFish + sizeFish, sizeFish);
        Grass = new(grass, Safari_Grass, Header.Grass);
        Surf = new(surf, Safari_Surf, Header.Surf);
        Old = new(old, Safari_Old_Rod, Header.Old);
        Good = new(good, Safari_Good_Rod, Header.Good);
        Super = new(super, Safari_Super_Rod, Header.Super);

        // Every file is 912 bytes because the header is always the same.
    }
}
