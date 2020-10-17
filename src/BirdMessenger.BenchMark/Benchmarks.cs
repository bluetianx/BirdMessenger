using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BirdMessenger.Collections;

namespace BirdMessenger.BenchMark
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        //[Benchmark]
        public async Task Scenario1()
        {
            var fileInfo = new FileInfo(@"TestFile/testf");
            MetadataCollection dir = new MetadataCollection();
            dir["filename"] = fileInfo.FullName;

            var fileUrl = await Program.tusClient.Create(fileInfo, dir);
            var uploadResult = await Program.tusClient.Upload(fileUrl, fileInfo, null);
        }

        [Benchmark]
        public async Task Scenario2()
        {
            var fileInfo = new FileInfo(@"TestFile/bigFile");
            MetadataCollection dir = new MetadataCollection();
            dir["filename"] = fileInfo.FullName;

            var fileUrl = await Program.tusClient.Create(fileInfo, dir);
            var uploadResult = await Program.tusClient.Upload(fileUrl, fileInfo, null);
        }
    }
}
