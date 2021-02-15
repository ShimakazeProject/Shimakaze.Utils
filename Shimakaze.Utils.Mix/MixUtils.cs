using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shimakaze.Utils.Mix.Struct;
using Shimakaze.Utils.Mix.Utils;

namespace Shimakaze.Utils.Mix
{
    public static class MixUtils
    {
        
        #region Constant

        internal const int MIX_CHECKSUM = 0x0001_0000;
        internal const int MIX_ENCRYPTED = 0x0002_0000;
        internal const int CB_MIX_KEY = 56;
        internal const int CB_MIX_KEY_SOURCE = 80;
        internal const int CB_MIX_CHECKSUM = 20;

        #endregion

        #region Public Methods

        public static async Task UnPack(string mixFilePath, string outFolderPath, string? listFilePath = default)
        {
            bool isChecksum, isEncrypted;
            int body_offset;
            MixHeader head;
            MixIndexEntry[] index;
            Dictionary<MixIndexEntry, string> files;
            Dictionary<uint, string>? fileNameMap = default;
            Memory<byte> buffer = new byte[CB_MIX_KEY_SOURCE];

            Console.WriteLine("Loading File Head");
            await using FileStream fs = new(mixFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            (isChecksum, isEncrypted) = await fs.GetFlag(buffer).ConfigureAwait(false);

            if (isChecksum)
                Console.WriteLine("This Mix File has Checksum.");
            if (isEncrypted)
            {
                Console.WriteLine("This Mix File was be Encrypted by Blowfish.");
                (head, index) = await fs.DencryptHead(buffer).ConfigureAwait(false);
            }
            else
            {
                head = await fs.GetHead(buffer).ConfigureAwait(false);
                index = await fs.GetIndex(buffer, head).ConfigureAwait(false);
            }
            body_offset = (int)fs.Position;

            Console.WriteLine($"This Mix File have {head.Files} file(s).");
            Console.WriteLine($"This Mix File have {head.Size} byte(s).");
            Console.WriteLine($"This Mix Body offset at 0x{body_offset:X8}.");

            // local xcc database.dat
            var lxd = index.Where(item => item.Id is 0x366E051F or 0x54C2D545);
            if (lxd.Any())
            {
                Console.WriteLine("Find local xcc database.dat File!");
                Console.WriteLine("Trying Generat FileName Map.");
                fileNameMap = await fs.LoadLocalXccDatabase(lxd.First(), body_offset).ConfigureAwait(false);
                if (fileNameMap is null)
                    Console.Error.WriteLine("Cannot Load local xcc database.dat File.");
            }

            // file.mix.map
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

            // make file map
            files = new(head.Files);
            Action<MixIndexEntry> parseFiles = fileNameMap is null
                ? (i => files.Add(i, $"0x{i.Id:X8}"))
                : (i => files.Add(i, fileNameMap.TryGetValue(i.Id, out var name) ? name : $"0x{i.Id:X8}"));
            index.ForEach(parseFiles);

            if (!Directory.Exists(outFolderPath))
                Directory.CreateDirectory(outFolderPath);

            await fs.Unpack(files, outFolderPath, body_offset).ConfigureAwait(false);
        }

        public static async Task Pack(string mixFilePath, string[] inputFiles, bool isTS = default, bool isEncrypt = default, string? key_source_path = default)
        {
            if (inputFiles.Length > short.MaxValue)
                throw new ArgumentException($"Too More! {short.MaxValue} < {inputFiles.Length}");

            var files = inputFiles.Select(i => new FileInfo(i)).ToList();

            {
                var a = files.Select(i => i.Length).Sum();
                if (a > int.MaxValue)
                    throw new ArgumentException($"Too Large! {int.MaxValue}byte < {a}byte");
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
            outfs.Write(isEncrypt ? new byte[] { 0, 0, 2, 0 } : new byte[4]);

            Console.WriteLine("Write FileHead...");
            {
                await using BinaryWriter bw = new(new MemoryStream());
                bw.Write(head);
                fileHeads.Values.ForEach(bw.Write);
                bw.BaseStream.Seek(0, SeekOrigin.Begin);
                if (isEncrypt)
                {
                    Console.WriteLine("Encrypting...");
                    byte[] key_source = new byte[CB_MIX_KEY_SOURCE];
                    if (!string.IsNullOrEmpty(key_source_path))
                        await LoadKeySourceFromFile(key_source_path, key_source).ConfigureAwait(false);
                    else
                        new Random().NextBytes(key_source);
                    var bf = CreateBlowFishFromKeySource(key_source);
                    await bw.BaseStream.EncryptTo(outfs, bf, new byte[8]).ConfigureAwait(false);
                }
                else
                    await bw.BaseStream.CopyToAsync(outfs).ConfigureAwait(false);
            }

            Console.WriteLine("Write FileBody...");
            await ms.FlushAsync().ConfigureAwait(false);
            ms.Seek(0, SeekOrigin.Begin);
            await ms.CopyToAsync(outfs);
            await outfs.FlushAsync().ConfigureAwait(false);

            Console.WriteLine("Write FileMap...");
            await using StreamWriter oflsw = new(mixFilePath + ".map");
            await oflsw.WriteFileMap(fileHeads).ConfigureAwait(false);

            Console.WriteLine("All Done!");
        }

        public static async Task Encrypt(string mixFilePath, string outFilePath, string? key_source_path = default)
        {
            byte[] buffer = new byte[8];
            byte[] key_source = new byte[CB_MIX_KEY_SOURCE];
            int flag = MIX_ENCRYPTED;

            if (!Directory.Exists(Path.GetDirectoryName(outFilePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(outFilePath)!);

            Console.WriteLine("Make Key");
            if (!string.IsNullOrEmpty(key_source_path))
                await LoadKeySourceFromFile(key_source_path, key_source).ConfigureAwait(false);
            else
                new Random().NextBytes(key_source);
            var bf = CreateBlowFishFromKeySource(key_source);

            await File.WriteAllBytesAsync(outFilePath + ".key.source", key_source).ConfigureAwait(false);

            Console.WriteLine("Open Mix File");
            await using FileStream fs = new(mixFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await using FileStream outfs = new(outFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            using BinaryReader br = new(fs);

            var (isChecksum, isEncrypted) = await fs.GetFlag(buffer).ConfigureAwait(false);
            if (isEncrypted)
                throw new Exception("It's Encrypted!");
            if (isChecksum)
                flag |= MIX_CHECKSUM;

            BitConverter.GetBytes(flag).CopyTo(buffer, 0);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buffer, 0, sizeof(int));

            await outfs.WriteAsync(buffer.AsMemory(0, sizeof(int))).ConfigureAwait(false);
            await outfs.WriteAsync(key_source.AsMemory()).ConfigureAwait(false);

            Console.WriteLine("Encrypting Block");
            await fs.EncryptTo(outfs, bf, buffer).ConfigureAwait(false);

            Console.WriteLine("Write Mix Body");
            await fs.CopyToAsync(outfs).ConfigureAwait(false);
            Console.WriteLine("All Done!");
        }

        public static async Task Dencrypt(string mixFilePath, string outFilePath)
        {
            bool isChecksum, isEncrypted;
            int flag = 0;
            MixHeader head;
            MixIndexEntry[] index;
            Memory<byte> buffer = new byte[CB_MIX_KEY_SOURCE];

            Console.WriteLine("Loading File Head");
            await using FileStream fs = new(mixFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            (isChecksum, isEncrypted) = await fs.GetFlag(buffer).ConfigureAwait(false);
            if (!isEncrypted)
                throw new Exception("It's Not Encrypted!");
            if (isChecksum)
                flag |= MIX_CHECKSUM;

            Console.WriteLine("Dencrypting...");
            await using FileStream outfs = new(outFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            using BinaryWriter bw = new(outfs);
            (head, index) = await fs.DencryptHead(buffer).ConfigureAwait(false);

            Console.WriteLine("Writing...");
            bw.Write(flag);
            bw.Write(head);
            index.ForEach(bw.Write);
            await fs.CopyToAsync(outfs).ConfigureAwait(false);
            Console.WriteLine("All Done!");
        }

        #endregion

        #region Tool Methods

        public static async Task LoadKeySourceFromFile(string key_source_path, Memory<byte> key_source)
        {
            if (key_source.Length < CB_MIX_KEY_SOURCE)
                throw new Exception("Buffer Too Short");

            await using FileStream kfs = new(key_source_path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (kfs.Length < CB_MIX_KEY_SOURCE)
                throw new Exception("key_source Too Short");
            if (kfs.Length > CB_MIX_KEY_SOURCE + 4)
                kfs.Seek(4, SeekOrigin.Begin);

            await kfs.ReadAsync(key_source.Slice(0, CB_MIX_KEY_SOURCE)).ConfigureAwait(false);
        }

        public static BlowFish CreateBlowFishFromKeySource(byte[] key_source)
        {
            byte[] key = new byte[CB_MIX_KEY];
            Native.GetBlowfishKey(key_source, key);
            return new(key);
        }

        public static async Task<(bool isChecksum, bool isEncrypted)> GetFlag(this Stream stream, Memory<byte> buffer)
        {
            if (buffer.Length < 4)
                throw new Exception("Buffer Too Short");
            await stream.ReadAsync(buffer.Slice(0, 4)).ConfigureAwait(false);
            var flag = buffer.Slice(0, 4).ToArray().ToStruct<uint>();
            return ((flag & MIX_CHECKSUM) > 0, (flag & MIX_ENCRYPTED) > 0);
        }

        public static async Task<MixHeader> GetHead(this Stream stream, Memory<byte> buffer)
        {
            if (buffer.Length < 6)
                throw new Exception("Buffer Too Short");
            await stream.ReadAsync(buffer.Slice(0, 6)).ConfigureAwait(false);
            return buffer.Slice(0, 6).ToArray().ToStruct<MixHeader>();
        }

        public static async Task<MixIndexEntry[]> GetIndex(this Stream stream, Memory<byte> buffer, MixHeader head)
        {
            if (buffer.Length < 12)
                throw new Exception("Buffer Too Short");

            var index = new MixIndexEntry[head.Files];
            for (int i = 0; i < head.Files; i++)
            {
                await stream.ReadAsync(buffer.Slice(0, 12)).ConfigureAwait(false);
                index[i] = buffer.Slice(0, 12).ToArray().ToStruct<MixIndexEntry>();
            }
            return index;
        }

        public static async Task<(MixHeader, MixIndexEntry[])> DencryptHead(this Stream stream, Memory<byte> buffer)
        {
            if (buffer.Length < CB_MIX_KEY_SOURCE)
                throw new Exception("Buffer Too Short");

            MixHeader head;
            await stream.ReadAsync(buffer.Slice(0, CB_MIX_KEY_SOURCE)).ConfigureAwait(false);
            var bf = CreateBlowFishFromKeySource(buffer.Slice(0, CB_MIX_KEY_SOURCE).ToArray());

            await stream.ReadAsync(buffer.Slice(0, 8)).ConfigureAwait(false);

            head = bf.Decrypt_ECB(buffer.Slice(0, 8).ToArray()).ToStruct<MixHeader>();
            int IndexByteCount = head.Files * 12;
            var blockCount = (int)Math.Ceiling((IndexByteCount + 6) / 8d);

            await using MemoryStream ms = new(IndexByteCount + 6);
            await ms.WriteAsync(buffer.Slice(6, 2)).ConfigureAwait(false);

            for (int i = 1; i < blockCount; i++)
            {
                await stream.ReadAsync(buffer.Slice(0, 8)).ConfigureAwait(false);
                await ms.WriteAsync(bf.Decrypt_ECB(buffer.Slice(0, 8).ToArray()).AsMemory()).ConfigureAwait(false);
            }
            ms.Seek(0, SeekOrigin.Begin);

            return (head, await ms.GetIndex(buffer, head).ConfigureAwait(false));
        }

        public static async Task<Dictionary<uint, string>?> LoadLocalXccDatabase(this Stream stream, MixIndexEntry lxd, int body_offset)
        {
            Dictionary<uint, string> fileNameMap;
            try
            {
                StringBuilder sb = new();
                Func<string, uint> GetId;
                byte ch;
                stream.Seek(lxd.Offset + body_offset, SeekOrigin.Begin);
                await using MemoryStream ms = new(lxd.Size);
                await stream.CopyToAsync(ms, lxd.Size).ConfigureAwait(false);
                ms.Seek(48, SeekOrigin.Begin);
                using BinaryReader msbr = new(ms, Encoding.ASCII);
                var count = msbr.ReadInt32();
                fileNameMap = new(count);
                GetId = lxd.Id is 0x366E051F
                    ? IdUtils.GetIdASCII
                    : IdUtils.GetIdTs;
                for (int i = 0; i < count; i++)
                {
                    sb.Clear();
                    while ((ch = msbr.ReadByte()) > 0)
                        sb.Append((char)ch);
                    var name = sb.ToString();
                    fileNameMap.Add(GetId(name), name);
                }
                return fileNameMap;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return null;
            }
        }

        public static async Task WriteFileMap(this TextWriter tw, Dictionary<FileInfo, MixIndexEntry> fileMap)
        {
            int num = 0;
            Console.WriteLine("==============================================================");
            Console.WriteLine("    No    |     ID     |   Offset   |    Size    |    Name    ");
            fileMap.ForEach(i =>
            {
                Console.WriteLine($" {num:D8} | 0x{i.Value.Id:X8} | 0x{i.Value.Offset:X8} | 0x{i.Value.Size:X8} | {i.Key.Name}");
                tw.WriteLine($"0x{i.Value.Id:X8} : {i.Key.Name}");
                num++;
            });
            Console.WriteLine("==============================================================");
            await tw.FlushAsync();
        }

        public static async Task Unpack(this Stream stream, Dictionary<MixIndexEntry, string> fileMap, string outFolderPath, int body_offset)
        {
            Console.WriteLine("Unpacking...");
            Console.WriteLine("==============================================================");
            Console.WriteLine("    No    |     ID     |   Offset   |    Size    |    Name    ");
            int num = 0;
            foreach (var file in fileMap)
            {
                Console.WriteLine($" {num:D8} | 0x{file.Key.Id:X8} | 0x{file.Key.Offset:X8} | 0x{file.Key.Size:X8} | {file.Value}");
                await using var ofs = File.Create(Path.Combine(outFolderPath, file.Value));
                if (file.Key.Size > 0)
                {
                    stream.Seek(file.Key.Offset + body_offset, SeekOrigin.Begin);
                    await stream.CopyToAsync(ofs, file.Key.Size).ConfigureAwait(false);
                }
                num++;
            }
            Console.WriteLine("==============================================================");
            Console.WriteLine("All Done!");
        }

        public static async Task EncryptTo(this Stream stream, Stream outStream, BlowFish bf, byte[] buffer)
        {
            await stream.ReadAsync(buffer.AsMemory(0, sizeof(short))).ConfigureAwait(false);
            var byteCount = buffer.ToStruct<short>() * 12 + 6;
            stream.Seek(-sizeof(short), SeekOrigin.Current);
            var blockCountFloor = byteCount / sizeof(long);
            for (int i = 0; i <= blockCountFloor; i++)
            {
                new byte[sizeof(long)].CopyTo(buffer, 0);
                await stream.ReadAsync(buffer.AsMemory(0, i == blockCountFloor ? byteCount % sizeof(long) : sizeof(long))).ConfigureAwait(false);
                bf.Encrypt_ECB(buffer.AsMemory(0, sizeof(long)).ToArray()).CopyTo(buffer, 0);
                await outStream.WriteAsync(buffer.AsMemory(0, sizeof(long))).ConfigureAwait(false);
            }
        }

        #endregion

    }
}