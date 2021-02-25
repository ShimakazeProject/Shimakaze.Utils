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
        public async Task AutoTestAsync()
        {
            Console.WriteLine("CSF 2 JSON");
            await CsfUtils.Convert(@"Resources\ra2md.csf", @"Out\Auto\ra2md.json").ConfigureAwait(false);

            Console.WriteLine("JSON 2 CSF");
            await CsfUtils.Convert(@"Out\Auto\ra2md.json", @"Out\Auto\ra2md.csf").ConfigureAwait(false);

            Console.WriteLine("Check");
            var newFile = new FileInfo(@"Out\Auto\ra2md.csf").Length;
            var source = new FileInfo(@"Resources\ra2md.csf").Length;
            if (!source.Equals(newFile))
                throw new Exception("File Not Equal");

            Console.WriteLine("All Test Pass!");
        }
        [TestMethod]
        public async Task V1TestAsync()
        {
            Console.WriteLine("CSF 2 JSON");
            await V1.Convert(@"Resources\ra2md.csf", @"Out\v1\ra2md.json").ConfigureAwait(false);

            Console.WriteLine("JSON 2 CSF");
            await V1.Convert(@"Out\v1\ra2md.json", @"Out\v1\ra2md.csf").ConfigureAwait(false);

            Console.WriteLine("CSF 2 JSON");
            await V1.Convert(@"Out\v1\ra2md.csf", @"Out\v1\ra2md.json").ConfigureAwait(false);

            Console.WriteLine("Check");
            var newFile = new FileInfo(@"Out\v1\ra2md.csf").Length;
            var source = new FileInfo(@"Resources\ra2md.csf").Length;
            if (!source.Equals(newFile))
                throw new Exception("File Not Equal");

            Console.WriteLine("All Test Pass!");
        }
        [TestMethod]
        public async Task V2TestAsync()
        {
            Console.WriteLine("CSF 2 JSON");
            await V2.Convert(@"Resources\ra2md.csf", @"Out\v2\ra2md.json").ConfigureAwait(false);

            Console.WriteLine("JSON 2 CSF");
            await V2.Convert(@"Out\v2\ra2md.json", @"Out\v2\ra2md.csf").ConfigureAwait(false);

            Console.WriteLine("CSF 2 JSON");
            await V1.Convert(@"Out\v2\ra2md.csf", @"Out\v2\ra2md.json").ConfigureAwait(false);

            Console.WriteLine("Check");
            var newFile = new FileInfo(@"Out\v2\ra2md.csf").Length;
            var source = new FileInfo(@"Resources\ra2md.csf").Length;
            if (!source.Equals(newFile))
                throw new Exception("File Not Equal");

            Console.WriteLine("All Test Pass!");
        }
    }
}
