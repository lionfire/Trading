// Test file to verify benchmark compilation - temporarily disabled
#if false
using System;
using LionFire.Trading.Indicators.Benchmarks.Indicators;

namespace LionFire.Trading.Indicators.Benchmarks
{
    public class TestCompilation
    {
        public static void TestBenchmarks()
        {
            // Test that all benchmark classes can be instantiated
            var rsiBenchmark = new RsiBenchmark();
            var bbBenchmark = new BollingerBandsBenchmark();
            var stochBenchmark = new StochasticBenchmark();
            var macdBenchmark = new MacdBenchmark(); // Placeholder

            Console.WriteLine("All benchmark classes compile successfully!");
            
            // Set some test parameters
            rsiBenchmark.DataSize = 1000;
            rsiBenchmark.Period = 14;
            rsiBenchmark.Condition = MarketCondition.Trending;
            
            bbBenchmark.DataSize = 1000;
            bbBenchmark.Period = 20;
            bbBenchmark.StandardDeviations = 2.0m;
            bbBenchmark.Condition = MarketCondition.Volatile;
            
            stochBenchmark.DataSize = 1000;
            stochBenchmark.FastPeriod = 14;
            stochBenchmark.SlowKPeriod = 3;
            stochBenchmark.SlowDPeriod = 3;
            stochBenchmark.Condition = MarketCondition.Sideways;
            
            macdBenchmark.DataSize = 1000;
            macdBenchmark.FastPeriod = 12;
            macdBenchmark.SlowPeriod = 26;
            macdBenchmark.SignalPeriod = 9;
            
            Console.WriteLine("All benchmark parameters configured successfully!");
        }
    }
}
#endif