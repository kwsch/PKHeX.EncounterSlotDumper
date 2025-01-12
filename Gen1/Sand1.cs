using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.EncounterSlotDumper;

public static class Sand1
{
    public static void MineGen1JPBU()
    {
        const int ofsWildRB = 0xCF61; // 0x6F2 length
        //GetEncounters(blue, ofsWildRB, "blue_jp");
        FixEncounters("jp_blue", ofsWildRB);

        const int ofsSuperRB = 0xEC49;
        FixSuperRB("jp_blue", ofsSuperRB);
    }

    public static void MineGen12()
    {
        byte[] red = File.ReadAllBytes("red.gb");
        byte[] blue = File.ReadAllBytes("blue.gb");
        byte[] yellow = File.ReadAllBytes("yellow.gbc");
        byte[] gold = File.ReadAllBytes("gold.gbc");
        byte[] crystal = File.ReadAllBytes("crystal.gbc");
        GetLVLEvo(red, 0x3B1D8, "rb");
        GetLVLEvo(yellow, 0x3B361, "y");
        GetLVLEvo2(gold, 0x429B3, "gs");
        GetLVLEvo2(crystal, 0x427A7, "c"); // 0x425B1

        var TMHMy = string.Join(", ", yellow.Skip(0x1232D).Take(0x37).Select(x => x.ToString("000")));
        var TMHMc = string.Join(", ", crystal.Skip(0x1167A).Take(0x39).Select(x => x.ToString("000")));

        const int ofsSuperRB = 0xE919;
        FixSuperRB("rb", ofsSuperRB);
        const int ofsSuperY = 0xE919;
        FixSuperY("yellow", ofsSuperY);

        const int ofsWildRB = 0xCEEB;
        GetEncounters(red, ofsWildRB, "red");
        GetEncounters(blue, ofsWildRB, "blue");

        const int ofsWildYellow = 0xCB95;
        GetEncounters(yellow, ofsWildYellow, "yellow");

        FixEncounters("red", ofsWildRB);
        FixEncounters("blue", ofsWildRB);
        FixEncounters("yellow", ofsWildYellow);

        Console.ReadLine();
    }

    private static void FixSuperRB(string name, int baseOffset)
    {
        string file = $"encounter_{name}_f.pkl";
        byte[] data = File.ReadAllBytes(file);
        List<int> offsets = [];
        int i = 0;
        while (data[i] != 0xFF)
        {
            int map = data[i++];
            int ptr = (data[i + 1] << 8) | data[i] | 0x8000;
            ptr -= baseOffset;
            offsets.Add(ptr);
            BitConverter.GetBytes((short)ptr).CopyTo(data, i);
            i += 2;
        }

        foreach (int x in offsets.Distinct())
        {
            int t = x;
            int count = data[t++];

            for (int z = 0; z < count; z++)
            {
                int lvl = data[t + (z * 2) + 0];
                int ofs = t + (z * 2) + 1;

                var spc = data[ofs];
                spc = GetG1Species(spc);
                data[ofs] = spc;
            }
        }
        File.WriteAllBytes(file + "fix", data);
    }

    private static void FixSuperY(string name, int baseOffset)
    {
        var fileName = $"encounter_{name}_f.pkl";
        var data = File.ReadAllBytes(fileName);

        int i = 0;
        while (data[i] != 0xFF)
        {
            int map = data[i++];
            const int count = 4;

            for (int z = 0; z < count; z++)
            {
                int lvl = data[i + (z * 2) + 0];
                int ofs = i + (z * 2) + 1;

                var spc = data[ofs];
                data[ofs] = GetG1Species(spc);
            }
            i += 2 * count;
        }
        File.WriteAllBytes(fileName + "fix", data);
    }

    private static void FixEncounters(string name, int baseOffset)
    {
        // Fix ptrs to little endian, and species IDs to natural numbers
        var fileName = $"encounter_{name}.pkl";
        var data = File.ReadAllBytes(fileName);

        int i = 0;
        var offsets = new List<int>();
        while (BitConverter.ToUInt16(data, i) != 0xFFFF)
        {
            int ptr = (data[i + 1] << 8) | data[i] | 0x8000;
            ptr -= baseOffset;
            offsets.Add(ptr);
            BitConverter.GetBytes((short)ptr).CopyTo(data, i);
            i += 2;
        }

        for (var index = 0; index < offsets.Count; index++)
        {
            int t = offsets[index];
            int ofs = t;

            // Check If Grass Data Present
            if (data[ofs++] != 0)
            {
                for (int j = 0; j < 20; j += 2)
                    data[ofs + j + 1] = GetG1Species(data[ofs + j + 1]);
                ofs += 20;
            }

            // Check If Water Data Present
            if (data[ofs++] != 0)
            {
                for (int j = 0; j < 20; j += 2)
                    data[ofs + j + 1] = GetG1Species(data[ofs + j + 1]);
                ofs += 20;
            }
        }

        File.WriteAllBytes(fileName + "fix", data);
    }

    private static void GetEncounters(ReadOnlySpan<byte> data, int offset, string name)
    {
        var areaptr = new List<int>();
        while (ReadUInt16LittleEndian(data[offset..]) != 0xFFFF)
        {
            areaptr.Add((data[offset + 1] << 8) | data[offset] | 0x8000);
            offset += 2;
        }

        int areas = areaptr.Count;
        int maxptr = areaptr.Max();
        var locs = new List<List<byte>>();
        for (int i = 0; i < areas; i++)
        {
            int ofs = areaptr[i];
            var loc = new List<byte>();

            byte b;
            // Get Regular Encounters

            loc.Add(b = data[ofs++]);
            if (b != 0)
            {
                for (int z = 0; z < 20; z++)
                    loc.Add(data[ofs++]);
            }
            loc.Add(b = data[ofs++]);
            if (b != 0)
            {
                for (int z = 0; z < 20; z++)
                    loc.Add(data[ofs++]);
            }
            locs.Add(loc);
        }
        Console.WriteLine($"Game: {name} {locs.Count}");
        var win = locs.SelectMany(a => a.ToArray()).ToArray();
        Console.WriteLine(win.Length);
        File.WriteAllBytes($"enc_{name}.pkl", win);
    }

    private static void GetLVLEvo2(byte[] data, int ofs, string name)
    {
        var evos = new byte[252][];
        evos[0] = new byte[1];
        var learnsets = new byte[252][];
        learnsets[0] = new byte[1];

        using (var ms = new MemoryStream(data))
        using (var br = new BinaryReader(ms))
        {
            br.BaseStream.Position = ofs;

            for (int i = 1; i < 252; i++)
            {
                byte b;

                var evo = new List<byte>();
                while ((b = br.ReadByte()) != 0)
                {
                    evo.Add(b);
                    evo.Add(br.ReadByte());
                    evo.Add(br.ReadByte());
                    if (b == 5)
                        evo.Add(br.ReadByte());
                }
                evo.Add(0);

                var learn = new List<byte>();
                while ((b = br.ReadByte()) != 0)
                    learn.Add(b);
                learn.Add(0);

                evos[i] = evo.Count > 0 ? [.. evo] : [];
                learnsets[i] = learn.Count > 0 ? [.. learn] : [];
            }
            Console.WriteLine((br.BaseStream.Position - ofs).ToString("X"));
        }

        var e = evos.SelectMany(x => x).ToArray();
        var l = learnsets.SelectMany(x => x).ToArray();
        File.WriteAllBytes($"evos_{name}.pkl", e);
        File.WriteAllBytes($"lvlmove_{name}.pkl", l);
    }

    private static void GetLVLEvo(byte[] data, int ofs, string name)
    {
        int i = 0;
        byte[][] evos = new byte[152][];
        evos[0] = new byte[1];
        byte[][] learnsets = new byte[152][];
        learnsets[0] = new byte[1];

        using (var ms = new MemoryStream(data))
        using (var br = new BinaryReader(ms))
        {
            br.BaseStream.Position = ofs;

            while (i < max - 1)
            {
                byte b;
                int index = indexes[i];

                var evo = new List<byte>();
                while ((b = br.ReadByte()) != 0)
                {
                    evo.Add(b);
                    switch (b)
                    {
                        case 1:
                            evo.Add(br.ReadByte());
                            evo.Add(GetG1Species(br.ReadByte()));
                            break;
                        case 2:
                            evo.Add(br.ReadByte());
                            evo.Add(br.ReadByte());
                            evo.Add(GetG1Species(br.ReadByte()));
                            break;
                        case 3:
                            evo.Add(br.ReadByte());
                            evo.Add(GetG1Species(br.ReadByte()));
                            break;
                    }
                }
                evo.Add(0);

                var learn = new List<byte>();
                while ((b = br.ReadByte()) != 0)
                    learn.Add(b);
                learn.Add(0);

                if (index < 152)
                {
                    evos[index] = evo.Count > 0 ? [.. evo] : [];
                    learnsets[index] = learn.Count > 0 ? [.. learn] : [];
                }
                i++;
            }
            Console.WriteLine((br.BaseStream.Position - ofs).ToString("X"));
        }

        var e = evos.SelectMany(x => x).ToArray();
        var l = learnsets.SelectMany(x => x).ToArray();
        File.WriteAllBytes($"evos_{name}.pkl", e);
        File.WriteAllBytes($"lvlmove_{name}.pkl", l);
    }

    private static byte GetG1Species(byte raw_id) => itemTable[raw_id];

    private static ReadOnlySpan<byte> itemTable =>
    [
        0x00, 0x70, 0x73, 0x20, 0x23, 0x15, 0x64, 0x22, 0x50, 0x02, 0x67, 0x6C, 0x66, 0x58, 0x5E,
        0x1D, 0x1F, 0x68, 0x6F, 0x83, 0x3B, 0x97, 0x82, 0x5A, 0x48, 0x5C, 0x7B, 0x78, 0x09, 0x7F, 0x72, 0x00,
        0x00, 0x3A, 0x5F, 0x16, 0x10, 0x4F, 0x40, 0x4B, 0x71, 0x43, 0x7A, 0x6A, 0x6B, 0x18, 0x2F, 0x36, 0x60,
        0x4C, 0x00, 0x7E, 0x00, 0x7D, 0x52, 0x6D, 0x00, 0x38, 0x56, 0x32, 0x80, 0x00, 0x00, 0x00, 0x53, 0x30,
        0x95, 0x00, 0x00, 0x00, 0x54, 0x3C, 0x7C, 0x92, 0x90, 0x91, 0x84, 0x34, 0x62, 0x00, 0x00, 0x00, 0x25,
        0x26, 0x19, 0x1A, 0x00, 0x00, 0x93, 0x94, 0x8C, 0x8D, 0x74, 0x75, 0x00, 0x00, 0x1B, 0x1C, 0x8A, 0x8B,
        0x27, 0x28, 0x85, 0x88, 0x87, 0x86, 0x42, 0x29, 0x17, 0x2E, 0x3D, 0x3E, 0x0D, 0x0E, 0x0F, 0x00, 0x55,
        0x39, 0x33, 0x31, 0x57, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 0x44, 0x00, 0x37, 0x61, 0x2A, 0x96, 0x8F, 0x81,
        0x00, 0x00, 0x59, 0x00, 0x63, 0x5B, 0x00, 0x65, 0x24, 0x6E, 0x35, 0x69, 0x00, 0x5D, 0x3F, 0x41, 0x11,
        0x12, 0x79, 0x01, 0x03, 0x49, 0x00, 0x76, 0x77, 0x00, 0x00, 0x00, 0x00, 0x4D, 0x4E, 0x13, 0x14, 0x21,
        0x1E, 0x4A, 0x89, 0x8E, 0x00, 0x51, 0x00, 0x00, 0x04, 0x07, 0x05, 0x08, 0x06, 0x00, 0x00, 0x00, 0x00,
        0x2B, 0x2C, 0x2D, 0x45, 0x46, 0x47, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00
    ];

    private const byte max = 190;

    private static ReadOnlySpan<byte> indexes =>
    [
        112, 115, 032, 035, 021, 100, 034, 080, 002, 103, 108, 102, 088, 094, 029, 031, 104, 111, 131, 059, 151, 130,
        090, 072, 092, 123, 120, 009, 127, 114, 152, 153, 058, 095, 022, 016, 079, 064, 075, 113, 067, 122, 106, 107,
        024, 047, 054, 096, 076, 154, 126, 155, 125, 082, 109, 156, 056, 086, 050, 128, 157, 158, 159, 083, 048, 149,
        160, 161, 162, 084, 060, 124, 146, 144, 145, 132, 052, 098, 163, 164, 165, 037, 038, 025, 026, 166, 167, 147,
        148, 140, 141, 116, 117, 168, 169, 027, 028, 138, 139, 039, 040, 133, 136, 135, 134, 066, 041, 023, 046, 061,
        062, 013, 014, 015, 170, 085, 057, 051, 049, 087, 171, 172, 010, 011, 012, 068, 173, 055, 097, 042, 150, 143,
        129, 174, 175, 089, 176, 099, 091, 177, 101, 036, 110, 053, 105, 178, 093, 063, 065, 017, 018, 121, 001, 003,
        073, 179, 118, 119, 180, 181, 182, 183, 077, 078, 019, 020, 033, 030, 074, 137, 142, 184, 081, 185, 186, 004,
        007, 005, 008, 006, 187, 188, 189, 190, 043, 044, 045, 069, 070, 071
    ];
}
