using System;

namespace PKHeX.EncounterSlotDumper
{
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

        internal static T[] ConcatAll<T>(params T[][] arr)
        {
            int len = 0;
            foreach (var a in arr)
                len += a.Length;

            var result = new T[len];

            int ctr = 0;
            foreach (var a in arr)
            {
                a.CopyTo(result, ctr);
                ctr += a.Length;
            }

            return result;
        }
    }
}