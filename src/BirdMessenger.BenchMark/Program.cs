using BenchmarkDotNet.Running;

namespace BirdMessenger.BenchMark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}