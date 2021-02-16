using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Shimakaze.Utils.Csf.Struct;

namespace Shimakaze.Utils.Csf.Utils
{
    public static class CsfUtil
    {
        public static async Task<CsfStruct> Deserialize(Stream stream, Memory<byte> buffer)
        {
            CsfStruct csf = new();
            csf.Head = await HeadUtil.Deserialize(stream, buffer).ConfigureAwait(false);
            csf.Data = new List<CsfLabel>(csf.Head.LabelCount);

            for (var i = 0; i < csf.Head.LabelCount; i++)
                csf.Data.Add(await LabelUtil.Deserialize(stream, buffer).ConfigureAwait(false));

            return csf;
        }

        public static async Task Serialize(this CsfStruct @this, Stream stream, byte[] buffer)
        {
            await @this.Head.Serialize(stream, buffer).ConfigureAwait(false);
            foreach (var i in @this.Data)
                await i.Serialize(stream, buffer).ConfigureAwait(false);
        }

        public static void ReCount(this CsfStruct @this)
        {
            @this.Head.LabelCount = @this.Data.Count;
            @this.Head.StringCount = @this.Data.Select(i => i.Values.Count).Sum();
        }
    }
}
