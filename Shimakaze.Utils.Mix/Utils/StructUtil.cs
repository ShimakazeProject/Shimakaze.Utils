using System.IO;

using Shimakaze.Utils.Mix.Struct;

namespace Shimakaze.Utils.Mix.Utils
{
    internal static class StructUtil
    {
        public static void Write(this BinaryWriter @this, MixHeader head)
        {
            @this.Write(head.Files);
            @this.Write(head.Size);
        }
        
        public static void Write(this BinaryWriter @this, MixIndexEntry head)
        {
            @this.Write(head.Id);
            @this.Write(head.Offset);
            @this.Write(head.Size);
        }
    }
}