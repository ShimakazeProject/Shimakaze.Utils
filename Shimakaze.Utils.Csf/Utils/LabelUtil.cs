using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Shimakaze.Utils.Csf.Struct;

namespace Shimakaze.Utils.Csf.Utils
{
    public static class LabelUtil
    {
        public const int FLAG_RAW = 1279413280;

        public static async Task<CsfLabel> Deserialize(Stream stream, Memory<byte> buffer)
        {
            int length;
            CsfLabel label = new();

            buffer.SizeCheck(12);
            await stream.ReadAsync(buffer.Slice(0, 12)).ConfigureAwait(false);
            unsafe
            {
                uint* ptr = (uint*)buffer.Pin().Pointer;
                if (FLAG_RAW != *ptr++)
                    throw new FormatException("Unknown File Format: Unknown Label Flag");
                label.Values = new List<CsfValue>(*(int*)ptr++);
                length = *(int*)ptr;
            }
            buffer.SizeCheck(length);
            await stream.ReadAsync(buffer.Slice(0, length)).ConfigureAwait(false);
            label.Label = Encoding.ASCII.GetString(buffer.Slice(0, length).Span);

            for (int i = 0; i < label.Values.Capacity; i++)
                label.Values.Add(await ValueUtil.Deserialize(stream, buffer).ConfigureAwait(false));

            return label;
        }

        public static async Task Serialize(this CsfLabel @this, Stream stream, byte[] buffer)
        {
            buffer.SizeCheck(12);
            FLAG_RAW.CopyToLittleEndianByteArray(buffer);
            @this.Values.Count.CopyToLittleEndianByteArray(buffer, 4);
            @this.Label.Length.CopyToLittleEndianByteArray(buffer, 8);

            await stream.WriteAsync(buffer.AsMemory(0, 12)).ConfigureAwait(false);
            await stream.WriteAsync(Encoding.ASCII.GetBytes(@this.Label).AsMemory()).ConfigureAwait(false);

            foreach (var i in @this.Values)
                await i.Serialize(stream, buffer).ConfigureAwait(false);

        }
    }
}
