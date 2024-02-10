namespace PKHeX.EncounterSlotDumper;

public interface IMagnetStatic
{
    byte StaticIndex { get; set; }
    byte MagnetPullIndex { get; set; }
    byte StaticCount { get; set; }
    byte MagnetPullCount { get; set; }
}

public static class MagnetStaticExtensions
{
    public static bool IsMatchStatic(this IMagnetStatic slot, int index, int count) => index == slot.StaticIndex && count == slot.StaticCount;
    public static bool IsMatchMagnet(this IMagnetStatic slot, int index, int count) => index == slot.MagnetPullIndex && count == slot.MagnetPullCount;
}

public interface INumberedSlot
{
    byte SlotNumber { get; set; }
}
