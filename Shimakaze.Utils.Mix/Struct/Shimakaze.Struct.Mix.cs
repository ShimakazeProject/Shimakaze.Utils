using System.Runtime.InteropServices;

namespace Shimakaze.Utils.Mix.Struct
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MixHeader
    {
        [FieldOffset(0)]
        public short Files;  // 文件数量
        [FieldOffset(sizeof(short))]
        public int Size;     // 主体大小

        [FieldOffset(0)]
        public int Flag;     // 文件标记

        public MixHeader(short files) : this()
        {
            Files = files;
        }
    }

    public struct MixIndexEntry
    {
        public uint Id;     // 文件校验值
        public int Offset;  // 文件偏移
        public int Size;    // 文件大小

        public MixIndexEntry(uint id, int offset, int size)
        {
            Id = id;
            Offset = offset;
            Size = size;
        }
    }
}