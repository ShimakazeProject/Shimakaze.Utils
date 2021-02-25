using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Shimakaze.Utils.Csf.Struct;

namespace Shimakaze.Utils.Csf.Utils
{
    public static class ValueUtil
    {
        public const int STR_RAW = 1398034976;
        public const int WSTR_RAW = 1398035031;

        public static async Task<CsfValue> Deserialize(Stream stream, Memory<byte> buffer)
        {
            int length, flag;
            CsfValue value = new();
            buffer.SizeCheck(12);

            await stream.ReadAsync(buffer.Slice(0, 4)).ConfigureAwait(false);
            unsafe
            {
                flag = *(int*)buffer.Pin().Pointer;
            }
            if (flag is not STR_RAW and not WSTR_RAW)
                throw new FormatException("Unknown File Format: Unknown Label Flag");

            await stream.ReadAsync(buffer.Slice(0, 4)).ConfigureAwait(false);
            unsafe
            {
                length = (*(int*)buffer.Pin().Pointer) << 1;
            }
            buffer.SizeCheck(length);
            await stream.ReadAsync(buffer.Slice(0, length)).ConfigureAwait(false);
            value.Value = Encoding.Unicode.GetString(Decoding(buffer.Slice(0, length).ToArray()));

            if (flag is WSTR_RAW)
            {
                await stream.ReadAsync(buffer.Slice(0, 4)).ConfigureAwait(false);
                unsafe
                {
                    length = *(int*)buffer.Pin().Pointer;
                }
                buffer.SizeCheck(length);
                await stream.ReadAsync(buffer.Slice(0, length)).ConfigureAwait(false);
                value.Extra = Encoding.ASCII.GetString(buffer.Slice(0, length).Span);
            }
            return value;
        }

        public static async Task Serialize(this CsfValue @this, Stream stream, byte[] buffer)
        {
            buffer.SizeCheck(8);
            var flag = string.IsNullOrEmpty(@this.Extra);
            var str = Encoding.Unicode.GetBytes(@this.Value.Replace("\r\n", "\n"));

            (flag ? STR_RAW : WSTR_RAW).CopyToLittleEndianByteArray(buffer);
            @this.Value.Length.CopyToLittleEndianByteArray(buffer, 4);

            await stream.WriteAsync(buffer.AsMemory(0, 8)).ConfigureAwait(false);
            await stream.WriteAsync(Decoding(str).AsMemory()).ConfigureAwait(false);

            if (!flag)
            {
                @this.Extra.Length.CopyToLittleEndianByteArray(buffer);
                await stream.WriteAsync(buffer.AsMemory(0, 4)).ConfigureAwait(false);
                await stream.WriteAsync(Encoding.ASCII.GetBytes(@this.Extra).AsMemory()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 值字符串 编/解码<br/>
        /// CSF文档中的Unicode编码内容都是按位异或的<br/>
        /// 这个方法使用for循环实现
        /// </summary>
        /// <param name="ValueData">内容</param>
        /// <returns>编/解码后的数组</returns>
        public static byte[] Decoding(byte[] ValueData) => Decoding(ValueData, ValueData.Length);

        /// <summary>
        /// 值字符串 编/解码<br/>
        /// CSF文档中的Unicode编码内容都是按位异或的<br/>
        /// 这个方法使用for循环实现
        /// </summary>
        /// <param name="ValueData">内容</param>
        /// <param name="ValueDataLength">内容长度</param>
        /// <returns>编/解码后的数组</returns>
        public static byte[] Decoding(byte[] ValueData, int ValueDataLength)
        {
            for (var i = 0; i < ValueDataLength; ++i)
                ValueData[i] = (byte)~ValueData[i];
            return ValueData;
        }
    }
}
