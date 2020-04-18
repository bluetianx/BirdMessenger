using System;
using BenchmarkDotNet.Running;

namespace BirdMessenger.BenchMark
{
    public class Program
    {
        public  static  Uri host = new Uri("http://localhost:5051/files");
        public static  TusClient tusClient=TusBuild.DefaultTusClientBuild(host)
            .Build();
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}