using System.IO;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper;

public static class Event1
{
    public static void DumpGen1()
    {
        var result = CsvConverter.ConvertCsvToPickle<EncounterEvent1>(Resources.event1);

        // VC encounter is at the start. Chunk our result into two halves and save separate files.
        const int vcCount = 1;
        const int vcLength = vcCount * EncounterEvent1.Size;
        File.WriteAllBytes("event1gb.pkl", result[vcLength..]);
        File.WriteAllBytes("event1vc.pkl", result[..vcLength]);
    }
}
