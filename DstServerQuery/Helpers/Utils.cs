using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DstServerQuery.Helpers;

public static class Utils
{
    public static int GetCharCount(ReadOnlySpan<char> input, char targetChar)
    {
        ref ushort r = ref Unsafe.As<char, ushort>(ref Unsafe.AsRef(input.GetPinnableReference()));

        int count = 0;
        int length = input.Length;
        Vector<ushort> testVector = new(targetChar);
        int index = 0;

        for (; index + Vector<ushort>.Count <= length; index += Vector<ushort>.Count)
        {
            var val = Vector.Equals(Vector.LoadUnsafe(ref Unsafe.Add(ref r, index)), testVector) & Vector<ushort>.One;
            count += Vector.Sum(val);
        }

        for (; index < length; index++)
        {
            count += input[index] == targetChar ? 1 : 0;
        }
        return count;
    }



    public static int GetHashCodeFast(ReadOnlySpan<char> bytes)
    {
        return GetHashCodeFast(MemoryMarshal.AsBytes(bytes));
    }

    public static int GetHashCodeFast(ReadOnlySpan<byte> bytes)
    {
        ref byte r = ref MemoryMarshal.GetReference(bytes);

        int offset = 0;
        int hash = 5381;
        var length = bytes.Length;


        if (bytes.Length < Vector<byte>.Count)
        {
            while (length >= 8)
            {
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 0).GetHashCode());
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 1).GetHashCode());
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 2).GetHashCode());
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 3).GetHashCode());
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 4).GetHashCode());
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 5).GetHashCode());
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 6).GetHashCode());
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 7).GetHashCode());

                length -= 8;
                offset += 8;
            }

            if (length >= 4)
            {
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 0).GetHashCode());
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 1).GetHashCode());
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 2).GetHashCode());
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset + 3).GetHashCode());

                length -= 4;
                offset += 4;
            }

            while (length > 0)
            {
                hash = unchecked((hash << 5) + hash ^ Unsafe.Add(ref r, offset).GetHashCode());

                length -= 1;
                offset += 1;
            }

            return hash;
        }

        Vector<int> current = new Vector<int>(hash);
        while (offset + Vector<byte>.Count <= length)
        {
            var vec = Vector.LoadUnsafe(ref Unsafe.Add(ref r, offset)).As<byte, int>();
            current = (current << 5) + current ^ vec;

            offset += Vector<byte>.Count;
        }

        Vector<byte> end = current.As<int, byte>();
        for (int i = 0; i < Vector<byte>.Count / 8; i += 8)
        {
            hash = unchecked((hash << 5) + hash ^ end[i + 0]);
            hash = unchecked((hash << 5) + hash ^ end[i + 1]);
            hash = unchecked((hash << 5) + hash ^ end[i + 2]);
            hash = unchecked((hash << 5) + hash ^ end[i + 3]);
            hash = unchecked((hash << 5) + hash ^ end[i + 4]);
            hash = unchecked((hash << 5) + hash ^ end[i + 5]);
            hash = unchecked((hash << 5) + hash ^ end[i + 6]);
            hash = unchecked((hash << 5) + hash ^ end[i + 7]);
        }

        while (offset < length)
        {
            if (length - offset > 8)
            {
                hash = unchecked((hash << 5) + hash ^ bytes[0]);
                hash = unchecked((hash << 5) + hash ^ bytes[1]);
                hash = unchecked((hash << 5) + hash ^ bytes[2]);
                hash = unchecked((hash << 5) + hash ^ bytes[3]);
                hash = unchecked((hash << 5) + hash ^ bytes[4]);
                hash = unchecked((hash << 5) + hash ^ bytes[5]);
                hash = unchecked((hash << 5) + hash ^ bytes[6]);
                hash = unchecked((hash << 5) + hash ^ bytes[7]);
                offset += 8;
                continue;
            }
            else if (length - offset > 4)
            {
                hash = unchecked((hash << 5) + hash ^ bytes[0]);
                hash = unchecked((hash << 5) + hash ^ bytes[1]);
                hash = unchecked((hash << 5) + hash ^ bytes[2]);
                hash = unchecked((hash << 5) + hash ^ bytes[3]);
                offset += 8;
                continue;
            }

            hash = unchecked((hash << 5) + hash ^ bytes[offset].GetHashCode());
            offset++;
        }

        return hash;
    }

}
