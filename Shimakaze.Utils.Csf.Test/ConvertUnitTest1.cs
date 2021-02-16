using System.Security.Cryptography;
using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Shimakaze.Utils.Csf.Test
{
    [TestClass]
    public class ConvertUnitTest1
    {
        [TestMethod]
        public async Task V1TestAsync()
        {
            Console.WriteLine("CSF 2 JSON");
            await V1.Convert(@"Resources\ra2md.csf", @"Out\ra2md.json").ConfigureAwait(false);

            Console.WriteLine("JSON 2 CSF");
            await V1.Convert(@"Out\ra2md.json", @"Out\ra2md.csf").ConfigureAwait(false);

            Console.WriteLine("Check");
            var newFile = new FileInfo(@"Out\ra2md.csf").Length;
            var source = new FileInfo(@"Resources\ra2md.csf").Length;
            if (!source.Equals(newFile))
                throw new Exception("File Not Equal");

            Console.WriteLine("All Test Pass!");
        }
    }
}
