using System;

namespace PKHeX.EncounterSlotDumper;

/// <summary>
/// <see cref="PersonalInfo"/> class with values from Generation 4 games.
/// </summary>
public sealed class PersonalInfoG4 : PersonalInfoG3
{
    public new const int SIZE = 0x2C;

    public PersonalInfoG4(byte[] data) : base(data)
    {
    }

    // Manually added attributes
    public override int FormCount { get => Data[0x29]; set { } }
    protected internal override int FormStatsIndex { get => BitConverter.ToUInt16(Data, 0x2A); set { } }
}
