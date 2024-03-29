using System;
using System.Collections.Generic;
using System.Linq;

namespace PKHeX.EncounterSlotDumper;

/// <summary>
/// Generation 2 Headbutt Trees on a given <see cref="Location"/> map.
/// </summary>
/// <remarks>
/// Pokemon Crystal Headbutt tree encounters by trainer id, based on mechanics described in
/// https://bulbapedia.bulbagarden.net/wiki/Headbutt_tree#Mechanics
/// </remarks>
public sealed class TreesArea
{
    private const int PivotCount = 10;
    private const int ModerateTreeCount = 5;

    public readonly byte Location;
    private readonly TreeEncounterAvailable[] PivotModerate;
    private readonly TreeEncounterAvailable[] PivotLow;

#if DEBUG
    private readonly TreeCoordinates[] ValidTrees;
    private readonly TreeCoordinates[] InvalidTrees;
#endif

    public IReadOnlyList<TreeEncounterAvailable> GetTrees(SlotType2 t) => t == SlotType2.Headbutt
        ? PivotModerate
        : PivotLow;

    private static readonly byte[][] TrainerModerateTreeIndex = GenerateModerateTreeIndex();
    internal static TreesArea[] GetArray(byte[][] entries) => entries.Select(z => new TreesArea(z)).ToArray();

    private static byte[][] GenerateModerateTreeIndex()
    {
        // A tree has a low encounter or moderate encounter base on the TID Pivot Index (TID % 10)
        // For every Trainer Pivot Index, calculate the moderate encounter trees (total of 5)
        var result = new byte[PivotCount][];
        for (int i = 0; i < PivotCount; i++)
        {
            var moderate = new byte[ModerateTreeCount];
            for (int j = 0; j < moderate.Length; j++)
                moderate[j] = (byte)((i + j) % PivotCount);
            Array.Sort(moderate);
            result[i] = moderate;
        }
        return result;
    }

    private TreesArea(ReadOnlySpan<byte> entry)
    {
        // Coordinates of trees were obtained with the program G2Map
        // ValidTrees are those accessible by the player
        Location = entry[0];

        var valid = new TreeCoordinates[entry[1]];
        var ofs = 2;
        for (int i = 0; i < valid.Length; i++, ofs += 2)
            valid[i] = new TreeCoordinates(entry[ofs], entry[ofs + 1]);

        // Invalid tress are trees that the player can not reach without cheating devices, like a tree beyond other trees
        var invalid = new TreeCoordinates[entry[ofs]];
        ofs++;
        for (int i = 0; i < invalid.Length; i++, ofs += 2)
            invalid[i] = new TreeCoordinates(entry[ofs], entry[ofs + 1]);

        CreatePivotLists(valid, invalid, out PivotModerate, out PivotLow);

#if DEBUG
        ValidTrees = valid;
        InvalidTrees = invalid;
#endif
    }

    private static void CreatePivotLists(TreeCoordinates[] valid, TreeCoordinates[] invalid, out TreeEncounterAvailable[] moderate, out TreeEncounterAvailable[] low)
    {
        // For legality purposes, only the tree index is needed.
        // Group the trees data by their index; trees that share indexes are indistinguishable from one another
        var TreeIndexValid = valid.Select(t => t.Index).Distinct().ToArray();
        var TreeIndexInvalid = invalid.Select(t => t.Index).Distinct().Except(TreeIndexValid).ToArray();

        Array.Sort(TreeIndexValid);
        Array.Sort(TreeIndexInvalid);

        // Check for every trainer pivot index if there are trees with moderate encounter and low encounter available in the area
        moderate = new TreeEncounterAvailable[PivotCount];
        low = new TreeEncounterAvailable[PivotCount];
        for (int i = 0; i < PivotCount; i++)
        {
            var TrainerModerateTrees = TrainerModerateTreeIndex[i];
            moderate[i] = GetIsAvailableModerate(TrainerModerateTrees, TreeIndexValid, TreeIndexInvalid);
            low[i] = GetIsAvailableLow(TrainerModerateTrees, TreeIndexValid, TreeIndexInvalid);
        }
    }

    private static TreeEncounterAvailable GetIsAvailableModerate(ReadOnlySpan<byte> moderate, ReadOnlySpan<byte> valid, ReadOnlySpan<byte> invalid)
    {
        if (valid.ContainsAny(moderate))
            return TreeEncounterAvailable.ValidTree;
        if (invalid.ContainsAny(moderate))
            return TreeEncounterAvailable.InvalidTree;
        return TreeEncounterAvailable.Impossible;
    }

    private static TreeEncounterAvailable GetIsAvailableLow(ReadOnlySpan<byte> moderate, ReadOnlySpan<byte> valid, ReadOnlySpan<byte> invalid)
    {
        if (valid.ContainsAnyExcept(moderate))
            return TreeEncounterAvailable.ValidTree;
        if (invalid.ContainsAnyExcept(moderate))
            return TreeEncounterAvailable.InvalidTree;
        return TreeEncounterAvailable.Impossible;
    }

#if DEBUG
    public IEnumerable<string> DumpLocation(string[] locationNames)
    {
        string loc = locationNames[Location];
        yield return $"Location: [{Location:00}] {loc}";
        yield return "Valid:";
        foreach (var tree in ValidTrees)
            yield return tree.ToString();
        yield return "Invalid:";
        foreach (var tree in InvalidTrees)
            yield return tree.ToString();
        yield return "===";
    }
#endif
}
