using System;
using PKHeX.EncounterSlotDumper;

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
        var surf = data[sizeGrass..(sizeGrass + sizeSurf)];
        var old = data[(sizeGrass + sizeSurf)..];
        var good = data[(sizeGrass + sizeSurf + sizeFish)..];
        var super = data[(sizeGrass + sizeSurf + sizeFish)..];
        Grass = new(grass, SlotType.Grass_Safari, Header.Grass);
        Surf = new(surf, SlotType.Surf_Safari, Header.Surf);
        Old = new(old, SlotType.Old_Rod_Safari, Header.Old);
        Good = new(good, SlotType.Good_Rod_Safari, Header.Good);
        Super = new(super, SlotType.Super_Rod_Safari, Header.Super);

        // Every file is 912 bytes because the header is always the same.
    }
}
