using BenchmarkDotNet.Running;
using PerformanceTest.MeteoriteLanding;

namespace PerformanceTest
{
    public class Program
    {

        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MeteoriteLandingBenchmark>();
            Console.WriteLine(summary);
        }
    }
}
