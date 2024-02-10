using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PKHeX.EncounterSlotDumper;

public static class Sand5
{
    private const int SIZE_ZD51 = 0x30;
    private const int SIZE_ZD52 = 0x30;

    public static void MineGen5()
    {
        // Load in unpacked NARCs
        byte[] zd51 = File.ReadAllBytes("zd51");
        byte[] zd52 = File.ReadAllBytes("zd52");

        byte[][] eW1 = Directory.GetFiles("eW1").Select(File.ReadAllBytes).ToArray();
        byte[][] eW2 = Directory.GetFiles("eW2").Select(File.ReadAllBytes).ToArray();
        byte[][] eB1 = Directory.GetFiles("eB1").Select(File.ReadAllBytes).ToArray();
        byte[][] eB2 = Directory.GetFiles("eB2").Select(File.ReadAllBytes).ToArray();

        byte[][] edW1 = Reserialize(zd51, eW1, SIZE_ZD51);
        byte[][] edW2 = Reserialize(zd52, eW2, SIZE_ZD52);
        byte[][] edB1 = Reserialize(zd51, eB1, SIZE_ZD51);
        byte[][] edB2 = Reserialize(zd52, eB2, SIZE_ZD52);

        File.WriteAllBytes("encounter_w.pkl", BinLinker.Pack(edW1, "51"));
        File.WriteAllBytes("encounter_b.pkl", BinLinker.Pack(edB1, "51"));
        File.WriteAllBytes("encounter_w2.pkl", BinLinker.Pack(edW2, "52"));
        File.WriteAllBytes("encounter_b2.pkl", BinLinker.Pack(edB2, "52"));
    }

    private static byte[][] Reserialize(byte[] zd51, byte[][] eW1, int size)
    {
        byte[][] edW1 = new byte[eW1.Length][];
        for (int i = 0; i < zd51.Length; i += size)
        {
            ushort pk = zd51[0x14 + i];
            if (pk == 0xFF)
                continue;

            List<byte> data = [];
            for (int b = 0; b < eW1[pk].Length; b += 0xE8) // len table
                data.AddRange(eW1[pk].Skip(8 + b).Take(0xE0));

            ushort id = zd51[0x1A + i];
            edW1[pk] = [.. BitConverter.GetBytes(id), .. data];
        }
        return edW1.Where(z => z != null).ToArray();
    }
}
