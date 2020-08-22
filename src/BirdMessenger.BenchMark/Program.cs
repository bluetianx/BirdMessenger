using System;
using BenchmarkDotNet.Running;

namespace BirdMessenger.BenchMark
{
    public class Program
    {
        public  static  Uri host = new Uri("http://localhost:5000/files");
        public static  ITusClient tusClient=TusBuild.DefaultTusClientBuild(host)
            .Build();
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}