using System.IO;

namespace PKHeX.EncounterSlotDumper;

internal record EncounterStatic8N(byte NestID, byte RankMin, byte RankMax, byte FlawlessIVs)
{
    public ushort Species { get; init; }
    public byte Form { get; init; }
    public sbyte Gender { get; init; } = -1;

    public AbilityPermission8 Ability { get; init; }
    public bool CanGigantamax { get; init; }

    public byte[] Write()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Species);
        bw.Write(Form);
        bw.Write(Gender);
        bw.Write((byte)Ability);
        bw.Write(CanGigantamax);
        bw.Write(NestID);
        bw.Write(RankMin);
        bw.Write(RankMax);
        bw.Write(FlawlessIVs);
        return ms.ToArray();
    }
}
