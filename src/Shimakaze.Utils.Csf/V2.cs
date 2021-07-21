using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Shimakaze.Utils.Csf.Struct;
using Shimakaze.Utils.Csf.Utils;
using Shimakaze.Utils.Csf.Utils.Json.V2;

namespace Shimakaze.Utils.Csf
{
    public static class V2
    {
        public static async Task Convert(string input, string? output = default, int bufferLength = 1024)
        {
            Func<Stream, Stream, byte[], Task> method;
            string extension;
            switch (Path.GetExtension(input)?.ToUpper())
            {
                case ".JSON":
                    Console.WriteLine("Convert Json to Csf Protocol 2");
                    method = Json2Csf;
                    extension = ".csf";
                    break;
                case ".CSF":
                    Console.WriteLine("Convert Csf to Json Protocol 2");
                    method = Csf2Json;
                    extension = ".json";
                    break;
                default:
                    throw new NotSupportedException();

            }
            if (string.IsNullOrEmpty(output))
                output = Path.GetFileNameWithoutExtension(input) + extension;

            if (!Directory.Exists(Path.GetDirectoryName(output)))
                Directory.CreateDirectory(Path.GetDirectoryName(output)!);

            Console.WriteLine("Converting...");
            await using var ifs = File.OpenRead(input);
            await using var ofs = File.Create(output);
            await method(ifs, ofs, new byte[bufferLength]).ConfigureAwait(false);
            await ofs.FlushAsync();
            Console.WriteLine("All Done!");
        }

        public static async Task Csf2Json(Stream input, Stream output, byte[] buffer)
            => await JsonSerializer.SerializeAsync(output,
                await CsfUtil.Deserialize(input, buffer).ConfigureAwait(false),
                CsfJsonConverterUtils.CsfJsonSerializerOptions).ConfigureAwait(false);

        public static async Task Json2Csf(Stream input, Stream output, byte[] buffer)
        {
            var tmp = await JsonSerializer.DeserializeAsync<CsfStruct>(input, CsfJsonConverterUtils.CsfJsonSerializerOptions).ConfigureAwait(false);
            Debug.Assert(tmp is not null);
            await tmp.Serialize(output, buffer).ConfigureAwait(false);
        }
    }
}
