using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shimakaze.Utils.Mix;

namespace Shimakaze.Utils.Test.Mix
{
    [TestClass]
    public class MixUtilsTest
    {
        [TestMethod]
        public async System.Threading.Tasks.Task UnPackTestAsync()
        {
            Console.WriteLine("Normal Mix Unpacking...");
            await MixUtils.UnPack(@"Resources\NormalTest.mix", @"Resources\NormalTest").ConfigureAwait(false);

            Console.WriteLine("Normal TS Mix Unpacking...");
            await MixUtils.UnPack(@"Resources\NormalTSTest.mix", @"Resources\NormalTSTest").ConfigureAwait(false);

            Console.WriteLine("Encrypted Mix Unpacking...");
            await MixUtils.UnPack(@"Resources\EncryptedTest.mix", @"Resources\EncryptedTest").ConfigureAwait(false);

            Console.WriteLine("Encrypted TS Mix Unpacking...");
            await MixUtils.UnPack(@"Resources\EncryptedTSTest.mix", @"Resources\EncryptedTSTest").ConfigureAwait(false);

            Console.WriteLine("All Test Pass!");
        }
        [TestMethod]
        public async System.Threading.Tasks.Task PackTestAsync()
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
    }
}
