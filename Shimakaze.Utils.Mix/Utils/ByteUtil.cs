using System;
using System.Runtime.InteropServices;

namespace Shimakaze.Utils.Mix.Utils
{
    internal static class ByteUtil
    {
        public static T ToStruct<T>(this byte[] bytes, int startIndex = default, int? size = default)
        {
            var _size = size is null ? bytes.Length : size.Value;

            IntPtr buffer = Marshal.AllocHGlobal(_size);
            try
            {
                Marshal.Copy(bytes, startIndex, buffer, _size);

                return Marshal.PtrToStructure<T>(buffer)!;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}