using System;

namespace PKHeX.EncounterSlotDumper;

public static class ArrayUtil
{
    public static T[] Slice<T>(this T[] src, int offset, int length)
    {
        var data = new T[length];
        Array.Copy(src, offset, data, 0, data.Length);
        return data;
    }

    public static T[][] Split<T>(this T[] data, int size)
    {
        var result = new T[data.Length / size][];
        for (int i = 0; i < data.Length; i += size)
            result[i / size] = data.Slice(i, size);
        return result;
    }
}
