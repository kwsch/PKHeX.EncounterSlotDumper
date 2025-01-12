using System;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper;

/// <summary>
/// <see cref="PersonalInfo"/> table (array).
/// </summary>
/// <remarks>
/// Serves as the main object that is accessed for stat data in a particular generation/game format.
/// </remarks>
public class PersonalTable
{
    public static readonly PersonalTable HGSS = new(Resources.personal_hgss, 4, 493);
    public static readonly PersonalTable E = new(Resources.personal_e, 3, 384);

    public PersonalTable(byte[] data, int format, int maxSpecies)
    {
        var get = GetConstructor(format);
        int size = GetEntrySize(format);
        byte[][] entries = data.Split(size);
        Table = new PersonalInfo[entries.Length];
        for (int i = 0; i < Table.Length; i++)
            Table[i] = get(entries[i]);

        MaxSpeciesID = maxSpecies;
    }
    private static Func<byte[], PersonalInfo> GetConstructor(int format) => format switch
    {
        3 => (z => new PersonalInfoG3(z)),
        4 => (z => new PersonalInfoG4(z)),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
    };

    private static int GetEntrySize(int format) => format switch
    {
        3 => PersonalInfoG3.SIZE,
        4 => PersonalInfoG4.SIZE,
        _ => -1,
    };

    private readonly PersonalInfo[] Table;

    /// <summary>
    /// Gets an index from the inner <see cref="Table"/> array.
    /// </summary>
    /// <remarks>Has built in length checks; returns empty (0) entry if out of range.</remarks>
    /// <param name="index">Index to retrieve</param>
    /// <returns>Requested index entry</returns>
    public PersonalInfo this[int index]
    {
        get
        {
            if (0 <= index && index < Table.Length)
                return Table[index];
            return Table[0];
        }
        set
        {
            if (index < 0 || index >= Table.Length)
                return;
            Table[index] = value;
        }
    }

    /// <summary>
    /// Maximum Species ID for the Table.
    /// </summary>
    public readonly int MaxSpeciesID;
}
