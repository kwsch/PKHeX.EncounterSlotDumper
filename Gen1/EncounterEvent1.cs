namespace PKHeX.EncounterSlotDumper;

public sealed record EncounterEvent1 : IWriteable
{
    public const int Size = 8;

    public byte Species { get; init; }
    public byte Level { get; init; }
    public byte Move1 { get; init; }
    public byte Move2 { get; init; }
    public byte Move3 { get; init; }
    public byte Move4 { get; init; }

    public byte Lang { get; init; }
    public byte Trainer { get; init; }

    public byte[] Write()
    {
        var arr = new byte[Size];
        arr[0] = Species;
        arr[1] = Level;
        arr[2] = Move1;
        arr[3] = Move2;
        arr[4] = Move3;
        arr[5] = Move4;
        arr[6] = Lang;
        arr[7] = Trainer;
        return arr;
    }

    // Don't serialize.
    public string OT { get; init; } = string.Empty;
    public ushort ID { get; init; }
    public string Comment { get; init; } = string.Empty;
}
