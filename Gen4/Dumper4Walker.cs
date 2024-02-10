using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PKHeX.EncounterSlotDumper.Properties;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.EncounterSlotDumper;

public static class Dumper4Walker
{
    public static void DumpGen4()
    {
        var d = Resources.walker_enc;
        WalkerSet[] arr = WalkerSet.ReadAll(d);
        WalkerSet.Compress(arr, "encounter_walker4.pkl");
        File.WriteAllText("walker.txt", WalkerSet.GetText(arr));
    }
}

public class WalkerSet
{
    private const int Mons = 6;
    private const int Drops = 10;

    public const int SIZE = 4 + (Mons * WalkerEntry.SIZE) + (Drops * DropEntry.SIZE) + 8; // 0xC0
    public readonly int Index;
    public readonly WalkerEntry[] Array = new WalkerEntry[Mons];
    public readonly DropEntry[] Table = new DropEntry[Drops];
    public readonly byte[] Footer; // 4 bytes ??
    public readonly uint UnlockWatts;

    public WalkerSet(ReadOnlySpan<byte> data)
    {
        Index = BitConverter.ToInt32(data);
        for (int i = 0; i < Mons; i++)
            Array[i] = WalkerEntry.Read(data.Slice(4 + (i * WalkerEntry.SIZE), WalkerEntry.SIZE));
        for (int i = 0; i < Drops; i++)
            Table[i] = DropEntry.Read(data.Slice(4 + (Mons * WalkerEntry.SIZE) + (i * DropEntry.SIZE), DropEntry.SIZE));

        const int footerStart = 4 + (Mons * WalkerEntry.SIZE) + (Drops * DropEntry.SIZE);
        Footer = data.Slice(footerStart, 4).ToArray();
        UnlockWatts = ReadUInt32LittleEndian(data[(footerStart + 4)..]);
    }

    public static WalkerSet[] ReadAll(ReadOnlySpan<byte> bytes)
    {
        var count = bytes.Length / SIZE;
        var result = new WalkerSet[count];
        for (int i = 0; i < count; i++)
            result[i] = new WalkerSet(bytes[(i * SIZE)..]);
        return result;
    }

    public override string ToString()
    {
        var result = $"Index: {Index:X4}{Environment.NewLine}";
        result += string.Join(Environment.NewLine, Array.Select(z => z.ToString()));
        result += Environment.NewLine;
        result += string.Join(Environment.NewLine, Table.Select(z => z.ToString()));
        result += Environment.NewLine;
        result += BitConverter.ToString(Footer);
        result += Environment.NewLine;
        result += UnlockWatts.ToString();
        return result;
    }

    public void Compress(Stream s)
    {
        foreach (var z in Array)
            s.Write(z.Compress());
    }

    public static void Compress(IReadOnlyList<WalkerSet> arr, string path)
    {
        using var fs = File.Create(path, SIZE * arr.Count);
        foreach (var z in arr)
            z.Compress(fs);
    }

    public static string GetText(WalkerSet[] arr)
    {
        return string.Join(Environment.NewLine, arr.Select(z => z.ToString()));
    }
}

public class DropEntry
{
    public const int SIZE = 0x6;
    public ushort Item;
    public ushort Steps;
    public ushort Rate;

    public static DropEntry Read(ReadOnlySpan<byte> data) => new()
    {
        Item = ReadUInt16LittleEndian(data),
        Steps = ReadUInt16LittleEndian(data[2..]),
        Rate = ReadUInt16LittleEndian(data[4..]),
    };

    public override string ToString()
    {
        return $"Item: {Item:X4} Steps: {Steps} Rate: {Rate}";
    }
}

public class WalkerEntry
{
    public const int SIZE = 0x14;
    public ushort Species;
    public ushort Level;
    public ushort Item;
    public byte U6; // Shiny? Unused
    public byte Gender;
    public ushort Move1;
    public ushort Move2;
    public ushort Move3;
    public ushort Move4;
    public ushort Steps;
    public ushort Rate;

    public static WalkerEntry Read(ReadOnlySpan<byte> data) => new()
    {
        Species = ReadUInt16LittleEndian(data),
        Level = ReadUInt16LittleEndian(data[0x2..]),
        Item = ReadUInt16LittleEndian(data[0x4..]),
        U6 = data[0x6],
        Gender = data[0x7],
        Move1 = ReadUInt16LittleEndian(data[0x8..]),
        Move2 = ReadUInt16LittleEndian(data[0xA..]),
        Move3 = ReadUInt16LittleEndian(data[0xC..]),
        Move4 = ReadUInt16LittleEndian(data[0xE..]),
        Steps = ReadUInt16LittleEndian(data[0x10..]),
        Rate = ReadUInt16LittleEndian(data[0x12..]),
    };

    public override string ToString()
    {
        return $"{(Species)Species,-12}\t{Level}\t{Item}\t{Gender}\t{Move1:000}\t{Move2:000}\t{Move3:000}\t{Move4:000}\t{Steps}\t{Rate}";
    }

    public byte[] Compress()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Species);
        bw.Write((byte)Level);
        //bw.Write(Item);
        //bw.Write(U5);
        //bw.Write(U6);
        bw.Write((byte)(PersonalTable.HGSS[Species].Genderless ? 2 : Gender)); // because raw data doesn't specify 2 consistently!
        bw.Write(Move1);
        bw.Write(Move2);
        bw.Write(Move3);
        bw.Write(Move4);
        //bw.Write(Steps);
        //bw.Write(Rate);
        return ms.ToArray();
    }
}
