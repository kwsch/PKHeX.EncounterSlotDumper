namespace PKHeX.EncounterSlotDumper;

public enum SafariSubZone4
{
    Plains = 0,
    Meadow = 1,
    Savannah = 2,
    Peak = 3,
    Rocky = 4,
    Wetland = 5,
    Forest = 6,
    Swamp = 7,
    Marshland = 8,
    Wasteland = 9,
    Mountain = 10,
    Desert = 11,
}

public static class SafariSubZone4Extensions
{
    public static bool IsWater(this SafariSubZone4 zone) => zone switch
    {
        SafariSubZone4.Meadow => true,
        SafariSubZone4.Rocky => true,
        SafariSubZone4.Wetland => true,
        SafariSubZone4.Swamp => true,
        SafariSubZone4.Marshland => true,
        _ => false,
    };
}
