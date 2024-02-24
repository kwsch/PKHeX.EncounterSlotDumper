using System.IO;
using PKHeX.EncounterSlotDumper.Properties;

namespace PKHeX.EncounterSlotDumper;

public static class Event2
{
    public static void DumpGen2()
    {
        var result = CsvConverter.ConvertCsvToPickle<EncounterEvent2>(Resources.event2);
        File.WriteAllBytes("event2.pkl", result); // GB Era only.
    }
}
