using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

namespace BirdMessenger.BenchMark
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        [Benchmark]
        public async Task Scenario1()
        {
            var fileInfo = new FileInfo(@"TestFile/testf");
            Dictionary<string, string> dir = new Dictionary<string, string>();
            dir["filename"] = fileInfo.FullName;

            var fileUrl = await Program.tusClient.Create(fileInfo, dir);
            var uploadResult = await Program.tusClient.Upload(fileUrl, fileInfo);
        }

        [Benchmark]
        public async Task Scenario2()
        {
            var fileInfo = new FileInfo(@"TestFile/bigFile");
            Dictionary<string, string> dir = new Dictionary<string, string>();
            dir["filename"] = fileInfo.FullName;

            var fileUrl = await Program.tusClient.Create(fileInfo, dir);
            var uploadResult = await Program.tusClient.Upload(fileUrl, fileInfo);
        }
    }
}
