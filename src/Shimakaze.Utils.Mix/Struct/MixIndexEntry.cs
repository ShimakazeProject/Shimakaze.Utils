namespace Shimakaze.Utils.Mix.Struct
{
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