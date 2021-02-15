using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Shimakaze.Utils.Mix.Test
{
    [TestClass]
    public class MixUtilsTest
    {
        [TestMethod]
        public async Task UnPackTestAsync()
        {
            Console.WriteLine("Normal Mix Unpacking...");
            await MixUtils.UnPack(@"Resources\NormalTest.mix", @"Out\NormalTest").ConfigureAwait(false);

            Console.WriteLine("Normal TS Mix Unpacking...");
            await MixUtils.UnPack(@"Resources\NormalTSTest.mix", @"Out\NormalTSTest").ConfigureAwait(false);

            Console.WriteLine("Encrypted Mix Unpacking...");
            await MixUtils.UnPack(@"Resources\EncryptedTest.mix", @"Out\EncryptedTest").ConfigureAwait(false);

            Console.WriteLine("Encrypted TS Mix Unpacking...");
            await MixUtils.UnPack(@"Resources\EncryptedTSTest.mix", @"Out\EncryptedTSTest").ConfigureAwait(false);

            Console.WriteLine("All Test Pass!");
        }
        [TestMethod]
        public async Task PackTestAsync()
        {
            var fileList = new[]{
                @"Resources\NormalTest.mix",
                @"Resources\NormalTSTest.mix",
                @"Resources\EncryptedTest.mix",
                @"Resources\EncryptedTSTest.mix",
            };
            Console.WriteLine("Normal Mix Packing...");
            await MixUtils.Pack(@"Out\NormalTest.mix", fileList).ConfigureAwait(false);

            Console.WriteLine("Normal TS Mix Packing...");
            await MixUtils.Pack(@"Out\NormalTSTest.mix", fileList, true).ConfigureAwait(false);

            // Console.WriteLine("Encrypted Mix Packing...");
            // await MixUtils.Pack(@"Out\EncryptedTest.mix", fileList, false, true).ConfigureAwait(false);

            // Console.WriteLine("Encrypted TS Mix Packing...");
            // await MixUtils.Pack(@"Out\EncryptedTSTest.mix", fileList, true, true).ConfigureAwait(false);

            Console.WriteLine("All Test Pass!");
        }
        [TestMethod]
        public async Task EncryptTestAsync()
        {
            Console.WriteLine("Use Key Encrypting...");
            await MixUtils.Encrypt(@"Resources\NormalTest.mix", @"Out\CopyKeyEncryptTest.mix", @"Resources\EncryptedTest.mix");
            Console.WriteLine("UnPacking...");
            await MixUtils.UnPack( @"Out\CopyKeyEncryptTest.mix", @"Out\CopyKeyEncryptTest").ConfigureAwait(false);

            Console.WriteLine("Random Key Encrypting...");
            await MixUtils.Encrypt(@"Resources\NormalTest.mix", @"Out\RandomKeyEncryptTest.mix");
            Console.WriteLine("UnPacking...");
            await MixUtils.UnPack( @"Out\RandomKeyEncryptTest.mix", @"Out\RandomKeyEncryptTest").ConfigureAwait(false);


            Console.WriteLine("All Test Pass!");
        }
    }
}
