using System.Collections.Generic;

namespace PKHeX.EncounterSlotDumper;

public sealed record Crossover7b
{
    public ushort Species { get; }
    public byte Form { get; }
    public byte LevelMin { get; }
    public byte LevelMax { get; }
    public byte FromArea { get; }
    public byte ToArea { get; }

    public Crossover7b(ushort species, byte min, byte max, byte fromarea, byte toarea)
    {
        Species = species;
        LevelMin = min;
        LevelMax = max;
        FromArea = fromarea;
        ToArea = toarea;
    }

    public static readonly List<Crossover7b> AllCrossovers = new()
    {
        // Species, min level, max level, from area, to area
        new (016, 03, 04, 03, 28 ), // Pidgey from Route 1 to Pallet Town
        new (019, 03, 04, 03, 28 ), // Rattata from Route 1 to Pallet Town
        new (043, 03, 04, 03, 28 ), // Oddish from Route 1 to Pallet Town
        new (069, 03, 04, 03, 28 ), // Bellsprout from Route 1 to Pallet Town
        new (006, 03, 56, 03, 28 ), // Charizard from Route 1 to Pallet Town
        new (016, 03, 56, 03, 28 ), // Pidgey from Route 1 to Pallet Town
        new (017, 03, 56, 03, 28 ), // Pidgeotto from Route 1 to Pallet Town
        new (018, 03, 56, 03, 28 ), // Pidgeot from Route 1 to Pallet Town
        new (149, 03, 56, 03, 28 ), // Dragonite from Route 1 to Pallet Town
        new (016, 03, 08, 04, 30 ), // Pidgey from Route 2 to Pewter City
        new (019, 03, 08, 04, 30 ), // Rattata from Route 2 to Pewter City
        new (043, 03, 08, 04, 30 ), // Oddish from Route 2 to Pewter City
        new (069, 03, 08, 04, 30 ), // Bellsprout from Route 2 to Pewter City
        new (004, 03, 08, 05, 06 ), // Charmander from Route 3 to Route 4
        new (019, 03, 08, 05, 06 ), // Rattata from Route 3 to Route 4
        new (023, 03, 08, 05, 06 ), // Ekans from Route 3 to Route 4
        new (027, 03, 08, 05, 06 ), // Sandshrew from Route 3 to Route 4
        new (056, 03, 08, 05, 06 ), // Mankey from Route 3 to Route 4
        new (016, 11, 16, 08, 33 ), // Pidgey from Route 6 to Vermilion City
        new (019, 11, 16, 08, 33 ), // Rattata from Route 6 to Vermilion City
        new (037, 11, 16, 08, 33 ), // Vulpix from Route 6 to Vermilion City
        new (039, 11, 16, 08, 33 ), // Jigglypuff from Route 6 to Vermilion City
        new (054, 11, 16, 08, 33 ), // Psyduck from Route 6 to Vermilion City
        new (058, 11, 16, 08, 33 ), // Growlithe from Route 6 to Vermilion City
        new (113, 11, 16, 08, 33 ), // Chansey from Route 6 to Vermilion City
        new (006, 03, 56, 08, 33 ), // Charizard from Route 6 to Vermilion City
        new (016, 03, 56, 08, 33 ), // Pidgey from Route 6 to Vermilion City
        new (017, 03, 56, 08, 33 ), // Pidgeotto from Route 6 to Vermilion City
        new (018, 03, 56, 08, 33 ), // Pidgeot from Route 6 to Vermilion City
        new (149, 03, 56, 08, 33 ), // Dragonite from Route 6 to Vermilion City
        new (016, 22, 27, 09, 34 ), // Pidgey from Route 7 to Celadon City
        new (017, 22, 27, 09, 34 ), // Pidgeotto from Route 7 to Celadon City
        new (019, 22, 27, 09, 34 ), // Rattata from Route 7 to Celadon City
        new (020, 22, 27, 09, 34 ), // Raticate from Route 7 to Celadon City
        new (037, 22, 27, 09, 34 ), // Vulpix from Route 7 to Celadon City
        new (038, 22, 27, 09, 34 ), // Ninetales from Route 7 to Celadon City
        new (058, 22, 27, 09, 34 ), // Growlithe from Route 7 to Celadon City
        new (064, 22, 27, 09, 34 ), // Kadabra from Route 7 to Celadon City
        new (137, 22, 27, 09, 34 ), // Porygon from Route 7 to Celadon City
        new (006, 03, 56, 09, 34 ), // Charizard from Route 7 to Celadon City
        new (016, 03, 56, 09, 34 ), // Pidgey from Route 7 to Celadon City
        new (017, 03, 56, 09, 34 ), // Pidgeotto from Route 7 to Celadon City
        new (018, 03, 56, 09, 34 ), // Pidgeot from Route 7 to Celadon City
        new (149, 03, 56, 09, 34 ), // Dragonite from Route 7 to Celadon City
        new (016, 13, 18, 13, 33 ), // Pidgey from Route 11 to Vermilion City
        new (019, 13, 18, 13, 33 ), // Rattata from Route 11 to Vermilion City
        new (020, 13, 18, 13, 33 ), // Raticate from Route 11 to Vermilion City
        new (072, 13, 18, 13, 33 ), // Tentacool from Route 11 to Vermilion City
        new (073, 13, 18, 13, 33 ), // Tentacruel from Route 11 to Vermilion City
        new (096, 13, 18, 13, 33 ), // Drowzee from Route 11 to Vermilion City
        new (113, 13, 18, 13, 33 ), // Chansey from Route 11 to Vermilion City
        new (116, 13, 18, 13, 33 ), // Horsea from Route 11 to Vermilion City
        new (117, 13, 18, 13, 33 ), // Seadra from Route 11 to Vermilion City
        new (122, 13, 18, 13, 33 ), // Mr. Mime from Route 11 to Vermilion City
        new (129, 13, 18, 13, 33 ), // Magikarp from Route 11 to Vermilion City
        new (016, 31, 36, 14, 15 ), // Pidgey from Route 12 to Route 13
        new (070, 31, 36, 14, 15 ), // Weepinbell from Route 12 to Route 13
        new (072, 31, 36, 14, 15 ), // Tentacool from Route 12 to Route 13
        new (073, 31, 36, 14, 15 ), // Tentacruel from Route 12 to Route 13
        new (083, 31, 36, 14, 15 ), // Farfetch’d from Route 12 to Route 13
        new (098, 31, 36, 14, 15 ), // Krabby from Route 12 to Route 13
        new (113, 31, 36, 14, 15 ), // Chansey from Route 12 to Route 13
        new (116, 31, 36, 14, 15 ), // Horsea from Route 12 to Route 13
        new (117, 31, 36, 14, 15 ), // Seadra from Route 12 to Route 13
        new (129, 31, 36, 14, 15 ), // Magikarp from Route 12 to Route 13
        new (072, 33, 38, 15, 14 ), // Tentacool from Route 13 to Route 12
        new (073, 33, 38, 15, 14 ), // Tentacruel from Route 13 to Route 12
        new (116, 33, 38, 15, 14 ), // Horsea from Route 13 to Route 12
        new (117, 33, 38, 15, 14 ), // Seadra from Route 13 to Route 12
        new (129, 33, 38, 15, 14 ), // Magikarp from Route 13 to Route 12
        new (016, 31, 36, 18, 34 ), // Pidgey from Route 16 to Celadon City
        new (019, 31, 36, 18, 34 ), // Rattata from Route 16 to Celadon City
        new (084, 31, 36, 18, 34 ), // Doduo from Route 16 to Celadon City
        new (085, 31, 36, 18, 34 ), // Dodrio from Route 16 to Celadon City
        new (113, 31, 36, 18, 34 ), // Chansey from Route 16 to Celadon City
        new (006, 03, 56, 18, 34 ), // Charizard from Route 16 to Celadon City
        new (016, 03, 56, 18, 34 ), // Pidgey from Route 16 to Celadon City
        new (017, 03, 56, 18, 34 ), // Pidgeotto from Route 16 to Celadon City
        new (018, 03, 56, 18, 34 ), // Pidgeot from Route 16 to Celadon City
        new (149, 03, 56, 18, 34 ), // Dragonite from Route 16 to Celadon City
        new (120, 37, 42, 21, 22 ), // Staryu from Route 19 to Route 20
        new (121, 37, 42, 21, 22 ), // Starmie from Route 19 to Route 20
        new (006, 03, 56, 22, 36 ), // Charizard from Route 20 to Cinnabar Island
        new (016, 03, 56, 22, 36 ), // Pidgey from Route 20 to Cinnabar Island
        new (017, 03, 56, 22, 36 ), // Pidgeotto from Route 20 to Cinnabar Island
        new (018, 03, 56, 22, 36 ), // Pidgeot from Route 20 to Cinnabar Island
        new (149, 03, 56, 22, 36 ), // Dragonite from Route 20 to Cinnabar Island
        new (130, 37, 42, 22, 21 ), // Gyarados from Route 20 to Route 19
        new (072, 37, 42, 23, 28 ), // Tentacool from Route 21 to Pallet Town
        new (073, 37, 42, 23, 28 ), // Tentacruel from Route 21 to Pallet Town
        new (120, 37, 42, 23, 28 ), // Staryu from Route 21 to Pallet Town
        new (121, 37, 42, 23, 28 ), // Starmie from Route 21 to Pallet Town
        new (129, 37, 42, 23, 28 ), // Magikarp from Route 21 to Pallet Town
        new (007, 09, 14, 27, 26 ), // Squirtle from Route 25 to Route 24
        new (043, 09, 14, 27, 26 ), // Oddish from Route 25 to Route 24
        new (048, 09, 14, 27, 26 ), // Venonat from Route 25 to Route 24
        new (052, 09, 14, 27, 26 ), // Meowth from Route 25 to Route 24
        new (054, 09, 14, 27, 26 ), // Psyduck from Route 25 to Route 24
        new (069, 09, 14, 27, 26 ), // Bellsprout from Route 25 to Route 24
    };
}
