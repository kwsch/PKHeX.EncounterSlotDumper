namespace PKHeX.EncounterSlotDumper;

public sealed record EncounterEvent2 : IWriteable
{
    public const int Size = 12;

    public byte Species { get; init; }
    public byte Level { get; init; } // Met Level, and Current level if not overriden (see below).
    public byte Move1 { get; init; }
    public byte Move2 { get; init; }
    public byte Move3 { get; init; }
    public byte Move4 { get; init; }
    public byte Met { get; init; } // Met Location
    public byte Current { get; init; } // Current Level

    public bool IsShiny { get; init; } // if true, set {specific IVs?}
    public bool IsEgg { get; init; } // if true, catch rate is 10

    public byte Lang { get; init; } // Language ID restriction (not exactly LanguageID)
    public byte Trainer { get; init; } // Trainer Name / ID group restriction

    public byte[] Write()
    {
        var arr = new byte[Size];
        arr[0] = Species;
        arr[1] = Level;
        arr[2] = Move1;
        arr[3] = Move2;
        arr[4] = Move3;
        arr[5] = Move4;
        arr[6] = Met;
        arr[7] = Current;
        arr[8] = (byte)(IsShiny ? 1 : 0);
        arr[9] = (byte)(IsEgg ? 1 : 0);
        arr[10] = Lang;
        arr[11] = Trainer;
        return arr;
    }

    // Don't serialize.
    public string OT { get; init; } = string.Empty;
    public ushort ID { get; init; }
    public string Comment { get; init; } = string.Empty;
}
