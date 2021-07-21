using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Shimakaze.Utils.Csf.Struct;
using Shimakaze.Utils.Csf.Utils;

namespace Shimakaze.Utils.Csf
{
    public static class CsfUtils
    {
        public static async Task Convert(string input, string? output = default, int bufferLength = 1024, int protocol = 2)
        {
            string extension;
            switch (Path.GetExtension(input)?.ToUpper())
            {
                case ".JSON":
                    Console.WriteLine("Convert Json to Csf Protocol Auto");
                    protocol = await GetProtocol(input).ConfigureAwait(false);
                    extension = ".csf";
                    break;
                case ".CSF":
                    Console.WriteLine("Convert Csf to Json Protocol Auto");
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
            Func<string, string, int, Task> method = protocol switch
            {
                1 => V1.Convert,
                2 => V2.Convert,
                _ => throw new NotSupportedException()
            };
            await method(input, output, bufferLength).ConfigureAwait(false);
            Console.WriteLine("All Done!");
        }

        public static async Task<int> GetProtocol(string jsonFile)
        {
            await using FileStream fs = new(jsonFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (await JsonSerializer.DeserializeAsync<Protocol>(fs, new()
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            })).Version;
        }
        struct Protocol
        {
            [JsonPropertyName("protocol")]
            public int Version { get; set; }
        }
    }
}
