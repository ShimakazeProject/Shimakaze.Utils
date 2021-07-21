using System;

namespace Shimakaze.Utils.Csf.Utils
{
    internal static class BufferUtil
    {
        public static void SizeCheck(this Memory<byte> buffer, int length)
        {
            if (buffer.Length < length)
                throw new Exception($"Buffer is Too Short! Need {length} bytes!");
        }
        public static void SizeCheck(this byte[] buffer, int length)
            => SizeCheck((Memory<byte>)buffer, length);

        public static void CopyToLittleEndianByteArray(this int i, byte[] buffer, int start = 0)
        {
            BitConverter.GetBytes(i).CopyTo(buffer.AsMemory(start, sizeof(int)));
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buffer, start, sizeof(int));
        }
    }
}
