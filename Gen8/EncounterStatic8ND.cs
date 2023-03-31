using System.IO;

namespace PKHeX.EncounterSlotDumper;

internal record EncounterStatic8ND(byte Level, byte DynamaxLevel, byte FlawlessIVs, RaidVersion Group = RaidVersion.SWSH)
{
    public ushort Species { get; init; }
    public byte Form { get; init; }
    public bool CanGigantamax { get; init; }
    public AbilityPermission8 Ability { get; init; }
    public Moveset Moves { get; init; }
    public byte Index { get; init; }
    public Shiny Shiny { get; init; }

    public bool IsGroup(byte game) => Group switch
    {
        RaidVersion.SW => game == 44, // SW
        RaidVersion.SH => game == 45, // SH
        _ => true,
    };

    public byte[] Write()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write(Species);
        bw.Write(Form);
      //bw.Write(Gender); NONE ARE SPECIFIC GENDER
        bw.Write((byte)Ability);

        bw.Write(Moves.Move1);
        bw.Write(Moves.Move2);
        bw.Write(Moves.Move3);
        bw.Write(Moves.Move4);

        bw.Write(Level);
        bw.Write((byte)(DynamaxLevel + (CanGigantamax ? 0x80 : 0)));
        bw.Write((byte)(FlawlessIVs | ((int)Shiny << 4)));
        bw.Write(Index);

        return ms.ToArray();
    }
}
