using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Shimakaze.Utils.Csf.Struct;

namespace Shimakaze.Utils.Csf.Utils
{
    public static class HeadUtil
    {
        #region Constant

        // 标准CSF文件标识符
        public const int CSF_FLAG = 1129530912;

        public static readonly string[] LanguageList = new[] {
            "en_US",
            "en_UK",
            "de",
            "fr",
            "es",
            "it",
            "jp",
            "Jabberwockie",
            "kr",
            "zh"
        };

        #endregion

        public static async Task<CsfHead> Deserialize(Stream stream, Memory<byte> buffer)
        {
            CsfHead result;
            buffer.SizeCheck(24);
            await stream.ReadAsync(buffer.Slice(0, 24)).ConfigureAwait(false);
            unsafe
            {
                int* head = (int*)buffer.Pin().Pointer;
                if (CSF_FLAG != *head++)
                    throw new FormatException("Unknown File Format: Unknown Header");
                result.Version = *head++;
                result.LabelCount = *head++;
                result.StringCount = *head++;
                result.Unknown = *head++;
                result.Language = *head++;
            }
            return result;
        }

        public static async Task Serialize(this CsfHead @this, Stream stream, byte[] buffer)
        {
            int lenght;
            unsafe
            {
                lenght = sizeof(CsfHead);
            }
            buffer.SizeCheck(lenght + sizeof(int));
            CSF_FLAG.CopyToLittleEndianByteArray(buffer);
            unsafe
            {
                Marshal.Copy(new IntPtr(&@this), buffer, sizeof(int), lenght);
            }
            await stream.WriteAsync(buffer.AsMemory(0, lenght + sizeof(int))).ConfigureAwait(false);
        }
    }
}