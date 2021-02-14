using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shimakaze.Utils.Mix.Struct;
using Shimakaze.Utils.Mix.Utils;

namespace Shimakaze.Utils.Mix
{
    public static class MixUtils
    {
        public const int MIX_CHECKSUM = 0x0001_0000;
        public const int MIX_ENCRYPTED = 0x0002_0000;
        public const int CB_MIX_KEY = 56;
        public const int CB_MIX_KEY_SOURCE = 80;
        public const int CB_MIX_CHECKSUM = 20;

        /// <summary>
        /// Unpack Util
        /// </summary>
        public static async Task UnPack(string mixFilePath, string outFolderPath, string? listFilePath = default)
        {
            bool isChecksum, isEncrypted;
            MixHeader head;
            List<MixIndexEntry> index;
            long body_offset;
            Dictionary<uint, string>? fileNameMap = default;
            Dictionary<MixIndexEntry, string> files;

            Console.WriteLine("Loading File Head");
            await using FileStream fs = new(mixFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using BinaryReader br = new(fs);

            (isChecksum, isEncrypted) = ParseFlag(br.ReadInt32());
            if (isChecksum)
                Console.WriteLine("This Mix File has Checksum.");
            if (isEncrypted)
            {
                Console.WriteLine("This Mix File was be Encrypted by Blowfish.");
                var key_source = br.ReadBytes(CB_MIX_KEY_SOURCE);
                var key = new byte[CB_MIX_KEY];
                Native.GetBlowfishKey(key_source, key);

                BlowFish bf = new(key);
                var buffer = bf.Decrypt_ECB(br.ReadBytes(8));

                head = BytesToStruct<MixHeader>(buffer);
                int IndexByteCount = head.Files * 12;
                var blockCount = (int)Math.Ceiling((IndexByteCount + 6) / 8d);

                await using MemoryStream ms = new(IndexByteCount + 6);
                await ms.WriteAsync(buffer.AsMemory(6, 2)).ConfigureAwait(false);

                for (int i = 1; i < blockCount; i++)
                {
                    buffer = bf.Decrypt_ECB(br.ReadBytes(8));
                    await ms.WriteAsync(buffer.AsMemory()).ConfigureAwait(false);
                }
                ms.Seek(0, SeekOrigin.Begin);
                using BinaryReader msbr = new(ms);
                index = new(head.Files);
                for (int i = 0; i < head.Files; i++)
                    index.Add(BytesToStruct<MixIndexEntry>(msbr.ReadBytes(12)));
            }
            else
            {
                head = BytesToStruct<MixHeader>(br.ReadBytes(6));
                index = new(head.Files);
                for (int i = 0; i < head.Files; i++)
                    index.Add(BytesToStruct<MixIndexEntry>(br.ReadBytes(12)));
            }

            body_offset = fs.Position;
            try
            {
                foreach (var item in index.Where(item => item.Id is 0x366E051F or 0x54C2D545))
                {
                    Console.WriteLine("Find local xcc database.dat File!");
                    Console.WriteLine("Trying Generat FileName Map.");
                    StringBuilder sb = new();
                    Func<string, uint> GetId;
                    byte ch;
                    int count;
                    string name;
                    fs.Seek(item.Offset + body_offset, SeekOrigin.Begin);
                    await using MemoryStream ms = new(item.Size);
                    await fs.CopyToAsync(ms, item.Size).ConfigureAwait(false);
                    ms.Seek(48, SeekOrigin.Begin);
                    using BinaryReader msbr = new(ms, Encoding.ASCII);
                    count = msbr.ReadInt32();
                    if (fileNameMap is null)
                        fileNameMap = new(count);
                    GetId = item.Id is 0x366E051F
                        ? IdUtils.GetIdASCII
                        : IdUtils.GetIdTs;
                    for (int i = 0; i < count; i++)
                    {
                        sb.Clear();
                        while ((ch = msbr.ReadByte()) > 0)
                            sb.Append((char)ch);
                        name = sb.ToString();
                        fileNameMap.Add(GetId(name), name);
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Cannot Load local xcc database.dat File.");
                Console.Error.WriteLine(ex);
            }

            if (!string.IsNullOrEmpty(listFilePath))
            {
                Console.WriteLine("Loading FileMap List");
                if (fileNameMap is null)
                    fileNameMap = new(head.Files);
                using StreamReader sr = new(listFilePath);
                while (!sr.EndOfStream)
                {
                    var kvp = sr.ReadLine()!.Split(":");
                    fileNameMap.Add(Convert.ToUInt32(kvp[0].Trim(), 16), kvp[1].Trim());
                }
            }

            files = new(head.Files);
            Action<MixIndexEntry> parseFiles = fileNameMap is null
                ? (i => files.Add(i, $"0x{i.Id:X8}"))
                : (i => files.Add(i, fileNameMap.TryGetValue(i.Id, out var name) ? name : $"0x{i.Id:X8}"));
            index.ForEach(parseFiles);

            if (!Directory.Exists(outFolderPath))
                Directory.CreateDirectory(outFolderPath);

            Console.WriteLine("Unpacking...");
            Console.WriteLine("========================================================");
            Console.WriteLine("    No    |     ID     |   Offset   |    Size    | Name");
            int num = 0;
            foreach (var file in files)
            {
                Console.WriteLine($" {num:D8} | 0x{file.Key.Id:X8} | 0x{file.Key.Offset:X8} | 0x{file.Key.Size:X8} | {file.Value}");
                await using var ofs = File.Create(Path.Combine(outFolderPath, file.Value));
                if (file.Key.Size > 0)
                {
                    fs.Seek(file.Key.Offset + body_offset, SeekOrigin.Begin);
                    await fs.CopyToAsync(ofs, file.Key.Size).ConfigureAwait(false);
                }
                num++;
            }
            Console.WriteLine("========================================================");
            Console.WriteLine("All Done!");
        }

        /// <summary>
        /// Make MIX Pack
        /// </summary>
        /// <param name="mixFilePath"></param>
        /// <param name="inputFiles"></param>
        /// <param name="isTS"></param>
        /// <param name="blowfish"></param>
        /// <returns></returns>
        public static async Task Pack(string mixFilePath, string[] inputFiles, bool isTS = default)
        {
            if (inputFiles.Length > short.MaxValue)
                throw new ArgumentException($"ToMore! {short.MaxValue} < {inputFiles.Length}");

            var files = inputFiles.Select(i => new FileInfo(i)).ToList();

            {
                var a = files.Select(i => i.Length).Sum();
                if (a > int.MaxValue)
                    throw new ArgumentException($"ToLarge! {int.MaxValue}byte < {a}byte");
            }

            MixHeader head = new((short)files.Count);

            Console.WriteLine("Creating FileMap...");
            Func<string, uint> GetId = isTS
                ? IdUtils.GetIdTs
                : IdUtils.GetIdASCII;

            Dictionary<FileInfo, MixIndexEntry> fileHeads = new(head.Files);
            await using MemoryStream ms = new();

            Console.WriteLine("Creating FileBody...");
            foreach (var file in files)
            {
                MixIndexEntry fileHead = new()
                {
                    Id = GetId(file.Name),
                    Offset = (int)ms.Position,
                    Size = (int)file.Length
                };
                fileHeads.Add(file, fileHead);
                await using var fs = file.OpenRead();
                await fs.CopyToAsync(ms).ConfigureAwait(false);
            }
            head.Size = (int)ms.Length;

            if (!Directory.Exists(Path.GetDirectoryName(mixFilePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(mixFilePath)!);

            await using FileStream outfs = new(mixFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            await using BinaryWriter bw = new(outfs);
            bw.Write(0);

            Console.WriteLine("Write FileHead...");
            bw.Write(head);
            fileHeads.Values.ForEach(bw.Write);

            Console.WriteLine("Write FileBody...");
            await ms.FlushAsync().ConfigureAwait(false);
            ms.Seek(0, SeekOrigin.Begin);
            await ms.CopyToAsync(outfs);
            await outfs.FlushAsync().ConfigureAwait(false);

            Console.WriteLine("Write FileMap...");
            await Task.Run(async () =>
            {
                await using FileStream outFileList = new(mixFilePath + ".map", FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                await using StreamWriter oflsw = new(outFileList);

                int num = 0;
                Console.WriteLine("========================================================");
                Console.WriteLine("    No    |     ID     |   Offset   |    Size    | Name");
                fileHeads.ForEach(i =>
                {
                    Console.WriteLine($" {num:D8} | 0x{i.Value.Id:X8} | 0x{i.Value.Offset:X8} | 0x{i.Value.Size:X8} | {i.Key.Name}");
                    oflsw.WriteLine($"0x{i.Value.Id:X8} : {i.Key.Name}");
                    num++;
                });
                Console.WriteLine("========================================================");
                await oflsw.FlushAsync();
            });

            Console.WriteLine("All Done!");
        }

        private static (bool isChecksum, bool isEncrypted) ParseFlag(int flag)
            => ((flag & MIX_CHECKSUM) > 0, (flag & MIX_ENCRYPTED) > 0);

        internal static T BytesToStruct<T>(byte[] bytes, int startIndex = default, int? size = default)
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

        private static void Write(this BinaryWriter @this, MixHeader head)
        {
            @this.Write(head.Files);
            @this.Write(head.Size);
        }

        private static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (var item in @this)
                action(item);
        }

        private static void Write(this BinaryWriter @this, MixIndexEntry head)
        {
            @this.Write(head.Id);
            @this.Write(head.Offset);
            @this.Write(head.Size);
        }
    }
}